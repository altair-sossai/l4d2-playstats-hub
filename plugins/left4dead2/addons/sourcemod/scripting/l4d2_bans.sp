#pragma semicolon 1
#pragma newdecls required

#include <sourcemod>
#include <l4d2_playstats_hub>

#define BANS_PATH "/api/bans"
#define BANS_LIFT_PATH "/api/bans/lift"
#define BANS_CONNECTIONS_PATH "/api/bans/connections"
#define BANS_PUBLIC_PATH "/bans"
#define SYNC_COMMAND "sm_playstats_ban_sync"
#define RETRY_INTERVAL 60.0
#define PENDING_MAX 64
#define PATH_LENGTH 64
#define COMMUNITY_ID_LENGTH 32
#define STEAM_ID_LENGTH 32
#define NAME_LENGTH 128
#define REASON_LENGTH 256
#define COMMAND_LENGTH 64
#define DATE_LENGTH 32
#define KICK_LENGTH 512

public Plugin myinfo =
{
    name        = "L4D2 - Bans",
    author      = "Altair Sossai",
    description = "Sends the bans of the server to L4D2 PlayStats Hub and keeps the banned players out",
    version     = "1.0.0",
    url         = "https://github.com/altair-sossai/l4d2-playstats-hub"
};

ArrayList g_hPendingPaths;
ArrayList g_hPendingBodies;

public void OnPluginStart()
{
    LoadTranslations("l4d2_bans.phrases");

    PlayStatsHub_CreateConVars();

    g_hPendingPaths = new ArrayList(ByteCountToCells(PATH_LENGTH));
    g_hPendingBodies = new ArrayList();

    RegServerCmd(SYNC_COMMAND, Command_Sync, "Asks L4D2 PlayStats Hub about the ban of a player: kicks the player when banned, removes the ban of the server when not");

    CreateTimer(RETRY_INTERVAL, Retry_Timer, _, TIMER_REPEAT);
}

public void OnClientAuthorized(int client, const char[] auth)
{
    if (IsFakeClient(client) || !PlayStatsHub_IsConfigured())
        return;

    char communityId[COMMUNITY_ID_LENGTH];
    if (!GetClientAuthId(client, AuthId_SteamID64, communityId, sizeof(communityId)))
        return;

    HTTPRequest request = PlayStatsHub_CreateRequest(BANS_CONNECTIONS_PATH);
    if (request == null)
        return;

    JSONObject connection = new JSONObject();
    connection.SetString("player", communityId);

    request.Post(connection, BanStatus_Response, GetClientUserId(client));

    delete connection;
}

public Action OnBanClient(int client, int time, int flags, const char[] reason, const char[] kick_message, const char[] command, any source)
{
    if (!(flags & BANFLAG_AUTHID) || client <= 0 || IsFakeClient(client))
        return Plugin_Continue;

    char communityId[COMMUNITY_ID_LENGTH];
    if (!GetClientAuthId(client, AuthId_SteamID64, communityId, sizeof(communityId)))
        return Plugin_Continue;

    char name[NAME_LENGTH];
    GetClientName(client, name, sizeof(name));

    SendBan(communityId, name, time, reason, command, source);

    return Plugin_Continue;
}

public Action OnBanIdentity(const char[] identity, int time, int flags, const char[] reason, const char[] command, any source)
{
    if (!(flags & BANFLAG_AUTHID))
        return Plugin_Continue;

    SendBan(identity, "", time, reason, command, source);

    return Plugin_Continue;
}

public Action OnRemoveBan(const char[] identity, int flags, const char[] command, any source)
{
    if (!(flags & BANFLAG_AUTHID))
        return Plugin_Continue;

    JSONObject lift = new JSONObject();

    lift.SetString("player", identity);
    lift.SetString("command", command);
    SetAdmin(lift, source);

    Send(BANS_LIFT_PATH, lift);

    return Plugin_Continue;
}

Action Command_Sync(int args)
{
    if (args < 1)
    {
        PrintToServer("Usage: %s <SteamID64>", SYNC_COMMAND);
        return Plugin_Handled;
    }

    char communityId[COMMUNITY_ID_LENGTH];
    GetCmdArg(1, communityId, sizeof(communityId));

    char path[PATH_LENGTH];
    FormatEx(path, sizeof(path), "%s/%s", BANS_PATH, communityId);

    HTTPRequest request = PlayStatsHub_CreateRequest(path);
    if (request != null)
        request.Get(BanSync_Response);

    return Plugin_Handled;
}

void SendBan(const char[] player, const char[] name, int time, const char[] reason, const char[] command, int source)
{
    JSONObject ban = new JSONObject();

    ban.SetString("player", player);
    ban.SetString("name", name);
    ban.SetInt("minutes", time);
    ban.SetString("reason", reason);
    ban.SetString("command", command);
    SetAdmin(ban, source);

    Send(BANS_PATH, ban);
}

void SetAdmin(JSONObject body, int source)
{
    if (source <= 0 || source > MaxClients || !IsClientInGame(source) || IsFakeClient(source))
        return;

    char communityId[COMMUNITY_ID_LENGTH];
    if (GetClientAuthId(source, AuthId_SteamID64, communityId, sizeof(communityId)))
        body.SetString("adminCommunityId", communityId);

    char name[NAME_LENGTH];
    GetClientName(source, name, sizeof(name));
    body.SetString("adminName", name);
}

void Send(const char[] path, JSONObject body)
{
    HTTPRequest request = PlayStatsHub_CreateRequest(path);
    if (request == null)
    {
        delete body;
        return;
    }

    DataPack pack = new DataPack();
    pack.WriteString(path);
    pack.WriteCell(body);

    request.Post(body, Send_Response, pack);
}

void Send_Response(HTTPResponse response, DataPack pack)
{
    pack.Reset();

    char path[PATH_LENGTH];
    pack.ReadString(path, sizeof(path));
    JSONObject body = view_as<JSONObject>(pack.ReadCell());

    delete pack;

    if (response.Status != HTTPStatus_Invalid && response.Status < HTTPStatus_InternalServerError)
    {
        delete body;
        return;
    }

    if (g_hPendingBodies.Length >= PENDING_MAX)
    {
        LogError("The site did not answer and %d bans are waiting; this one was dropped", PENDING_MAX);
        delete body;
        return;
    }

    g_hPendingPaths.PushString(path);
    g_hPendingBodies.Push(body);
}

Action Retry_Timer(Handle timer)
{
    int count = g_hPendingBodies.Length;
    if (count == 0 || !PlayStatsHub_IsConfigured())
        return Plugin_Continue;

    char path[PATH_LENGTH];

    for (int i = 0; i < count; i++)
    {
        g_hPendingPaths.GetString(0, path, sizeof(path));
        JSONObject body = view_as<JSONObject>(g_hPendingBodies.Get(0));

        g_hPendingPaths.Erase(0);
        g_hPendingBodies.Erase(0);

        Send(path, body);
    }

    return Plugin_Continue;
}

void BanStatus_Response(HTTPResponse response, int userId)
{
    if (response.Status != HTTPStatus_OK || !PlayStatsHub_HasJson(response))
        return;

    int client = GetClientOfUserId(userId);
    if (client <= 0 || !IsClientConnected(client) || IsClientInKickQueue(client))
        return;

    Enforce(client, view_as<JSONObject>(response.Data));
}

void BanSync_Response(HTTPResponse response, any value)
{
    if (response.Status != HTTPStatus_OK || !PlayStatsHub_HasJson(response))
        return;

    JSONObject result = view_as<JSONObject>(response.Data);

    char communityId[COMMUNITY_ID_LENGTH];
    result.GetString("communityId", communityId, sizeof(communityId));

    if (!result.GetBool("banned"))
    {
        char steamId[STEAM_ID_LENGTH];

        result.GetString("steamId", steamId, sizeof(steamId));
        ServerCommand("removeid %s", steamId);

        result.GetString("legacySteamId", steamId, sizeof(steamId));
        ServerCommand("removeid %s", steamId);

        ServerCommand("writeid");
        return;
    }

    int client = FindClient(communityId);
    if (client > 0 && !IsClientInKickQueue(client))
        Enforce(client, result);
}

void Enforce(int client, JSONObject result)
{
    if (!result.GetBool("banned"))
        return;

    char reason[REASON_LENGTH];
    result.GetString("reason", reason, sizeof(reason));

    char site[PLAYSTATS_HUB_URL_LENGTH];
    PlayStatsHub_GetPublicUrl(site, sizeof(site));

    char message[KICK_LENGTH];

    if (result.GetBool("permanent"))
        FormatEx(message, sizeof(message), "%T", reason[0] == '\0' ? "BannedPermanently" : "BannedPermanentlyWithReason", client, reason, site, BANS_PUBLIC_PATH);
    else
    {
        char endsAt[DATE_LENGTH];
        result.GetString("endsAt", endsAt, sizeof(endsAt));

        FormatEx(message, sizeof(message), "%T", reason[0] == '\0' ? "BannedUntil" : "BannedUntilWithReason", client, endsAt, reason, site, BANS_PUBLIC_PATH);
    }

    int minutes = result.GetInt("minutesLeft");
    char steamId[STEAM_ID_LENGTH];

    result.GetString("steamId", steamId, sizeof(steamId));
    ServerCommand("banid %d %s", minutes, steamId);

    result.GetString("legacySteamId", steamId, sizeof(steamId));
    ServerCommand("banid %d %s", minutes, steamId);

    KickClient(client, "%s", message);
}

int FindClient(const char[] communityId)
{
    char current[COMMUNITY_ID_LENGTH];

    for (int client = 1; client <= MaxClients; client++)
    {
        if (!IsClientConnected(client) || IsFakeClient(client))
            continue;

        if (GetClientAuthId(client, AuthId_SteamID64, current, sizeof(current)) && StrEqual(current, communityId))
            return client;
    }

    return 0;
}
