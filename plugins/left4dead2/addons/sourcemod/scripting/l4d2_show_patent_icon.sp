#pragma semicolon 1
#pragma newdecls required

#include <sourcemod>
#include <sdktools>
#include <sdkhooks>
#include <left4dhooks>
#include <l4d2_playstats_hub>

#undef REQUIRE_PLUGIN
#include <readyup>
#define REQUIRE_PLUGIN

#define PATENTS_PATH "/api/players/patents"
#define COMMUNITY_ID_LENGTH 25

#define L4D2_TEAM_SURVIVOR 2
#define L4D2_TEAM_INFECTED 3

#define ICON_ORIGIN_SURVIVOR { -3.0, 0.0, 20.0 }
#define ICON_ORIGIN_INFECTED { -3.0, 0.0, 95.0 }

public Plugin myinfo =
{
    name        = "L4D2 - Show Patent Icon",
    author      = "Altair Sossai",
    description = "Show the patent icon of the players",
    version     = "2.0.0",
    url         = "https://github.com/altair-sossai/l4d2-playstats-hub"
};

ConVar
    hIconVersion,
    hMaxLevel;

int iconRef[MAXPLAYERS + 1] = { -1, ... };
int playersLevel[MAXPLAYERS + 1] = { 0, ... };

bool patentIconEnabled = true;

StringMap PlayersPatent;

public void OnPluginStart()
{
    PlayStatsHub_CreateConVars();

    hIconVersion = CreateConVar("patent_icon_version", "1", "Version of the patent icon", FCVAR_PROTECTED);
    hMaxLevel = CreateConVar("patent_icon_max_level", "15", "Max level of the patent", FCVAR_PROTECTED);

    PlayersPatent = new StringMap();

    HookEvent("player_team", PlayerTeam_Event, EventHookMode_Post);
    HookEvent("round_start", RoundStart_Event, EventHookMode_PostNoCopy);

    CreateTimer(1.0, PatentIconTick, _, TIMER_REPEAT);
}

public void OnRoundIsLive()
{
    patentIconEnabled = false;
    RemoveAllPatentIcons();
}

public void OnMapStart()
{
    PrecacheAllPatentFiles();
    RefreshPlayersPatent();
}

public void OnClientPutInServer(int client)
{
    if (patentIconEnabled && !IsFakeClient(client))
        RefreshPlayersLevel();
}

public void OnClientDisconnect(int client)
{
    if (!patentIconEnabled)
        return;

    RemovePatentIcon(client);
    RefreshPlayersLevel();
}

void PlayerTeam_Event(Event event, const char[] name, bool dontBroadcast)
{
    if (!patentIconEnabled)
        return;

    RemovePatentIcon(GetClientOfUserId(GetEventInt(event, "userid")));
}

void RoundStart_Event(Event hEvent, const char[] eName, bool dontBroadcast)
{
    CreateTimer(10.0, NewGameTick);
}

Action NewGameTick(Handle timer)
{
    patentIconEnabled = IsNewGame();

    if (patentIconEnabled)
        RefreshPlayersPatent();

    return Plugin_Stop;
}

Action PatentIconTick(Handle timer)
{
    if (!patentIconEnabled)
        return Plugin_Continue;

    int entity;
    int team;

    for (int i = 1; i <= MaxClients; i++)
    {
        if (!IsClientInGame(i) || IsFakeClient(i) || playersLevel[i] == 0)
        {
            RemovePatentIcon(i);
            continue;
        }

        team = GetClientTeam(i);
        if (team != L4D2_TEAM_SURVIVOR && team != L4D2_TEAM_INFECTED)
        {
            RemovePatentIcon(i);
            continue;
        }

        entity = EntRefToEntIndex(iconRef[i]);
        if (entity <= MaxClients || !IsValidEntity(entity))
            SetPatentIcon(i);
    }

    return Plugin_Continue;
}

void SetPatentIcon(int client)
{
    int entity = CreateEntityByName("env_sprite");
    if (entity <= MaxClients)
        return;

    iconRef[client] = EntIndexToEntRef(entity);

    char fileVMT[64];

    Format(fileVMT, sizeof(fileVMT), "materials/sprites/patent_%02d_v%02d.vmt", playersLevel[client], hIconVersion.IntValue);

    DispatchKeyValue(entity, "model", fileVMT);
    DispatchKeyValueFloat(entity, "scale", 0.001);
    DispatchSpawn(entity);

    SetEntProp(entity, Prop_Send, "m_nSolidType", 0);
    SetEntProp(entity, Prop_Send, "m_usSolidFlags", 4);
    SetEntProp(entity, Prop_Send, "m_CollisionGroup", 0);
    AcceptEntityInput(entity, "DisableCollision");

    SetEntityRenderMode(entity, RENDER_WORLDGLOW);

    SetVariantString("!activator");
    AcceptEntityInput(entity, "SetParent", client);
    SetVariantString("eyes");
    AcceptEntityInput(entity, "SetParentAttachment");
    
    float origin[3];
    
    if (GetClientTeam(client) == L4D2_TEAM_SURVIVOR)
        origin = ICON_ORIGIN_SURVIVOR;
    else
        origin = ICON_ORIGIN_INFECTED;

    TeleportEntity(entity, origin, NULL_VECTOR, NULL_VECTOR);

    SDKUnhook(entity, SDKHook_SetTransmit, OnSetIconTransmit);
    SDKHook(entity, SDKHook_SetTransmit, OnSetIconTransmit);
}

Action OnSetIconTransmit(int entity, int client)
{
    if (!patentIconEnabled)
        return Plugin_Handled;

    if (GetClientTeam(client) == L4D2_TEAM_SURVIVOR)
    {
        int ref = EntIndexToEntRef(entity);
        
        for (int i = 1; i <= MaxClients; i++)
            if (iconRef[i] == ref && GetClientTeam(i) != L4D2_TEAM_SURVIVOR)
                return Plugin_Handled;
    }

    return Plugin_Continue;
}

void RefreshPlayersPatent()
{
    HTTPRequest request = PlayStatsHub_CreateRequest(PATENTS_PATH);
    if (request == null)
        return;

    request.Get(RefreshPlayersPatentResponse);
}

void RefreshPlayersPatentResponse(HTTPResponse httpResponse, any value)
{
    if (httpResponse.Status != HTTPStatus_OK || !PlayStatsHub_HasJson(httpResponse))
        return;

    JSONArray response = view_as<JSONArray>(httpResponse.Data);
    if (response == null)
        return;

    PlayersPatent.Clear();

    char communityId[COMMUNITY_ID_LENGTH];

    for (int i = 0; i < response.Length; i++)
    {
        JSONObject player = view_as<JSONObject>(response.Get(i));

        if (player.GetString("communityId", communityId, sizeof(communityId)) && player.HasKey("level"))
            PlayersPatent.SetValue(communityId, player.GetInt("level"), true);

        delete player;
    }

    RefreshPlayersLevel();
}

bool IsNewGame()
{
    return L4D2Direct_GetVSCampaignScore(0) == 0 && L4D2Direct_GetVSCampaignScore(1) == 0;
}

void PrecacheAllPatentFiles()
{
    int patentIconVersion = hIconVersion.IntValue;
    int maxPatentLevel = hMaxLevel.IntValue;

    for (int i = 1; i <= maxPatentLevel; i++)
    {
        char fileVMT[64];
        char fileVTF[64];

        Format(fileVMT, sizeof(fileVMT), "materials/sprites/patent_%02d_v%02d.vmt", i, patentIconVersion);
        Format(fileVTF, sizeof(fileVTF), "materials/sprites/patent_%02d_v%02d.vtf", i, patentIconVersion);

        AddFileToDownloadsTable(fileVMT);
        AddFileToDownloadsTable(fileVTF);

        if (!IsModelPrecached(fileVMT))
            PrecacheModel(fileVMT, true);

        if (!IsModelPrecached(fileVTF))
            PrecacheModel(fileVTF, true);
    }
}

void RemoveAllPatentIcons()
{
    for (int i = 0; i <= MaxClients; i++)
        RemovePatentIcon(i);
}

void RemovePatentIcon(int client)
{
    int ent = EntRefToEntIndex(iconRef[client]);
    if (ent > MaxClients && IsValidEntity(ent))
    {
        SetEntProp(ent, Prop_Data, "m_fEffects", 32);
        RemoveEntity(ent);
    }
    iconRef[client] = -1;
}

void RefreshPlayersLevel()
{
    RemoveAllPatentIcons();

    for (int i = 1; i <= MaxClients; i++)
    {
        if (!IsClientInGame(i) || IsFakeClient(i))
        {
            playersLevel[i] = 0;
            continue;
        }

        char communityId[COMMUNITY_ID_LENGTH];
        int level;

        if (GetClientAuthId(i, AuthId_SteamID64, communityId, sizeof(communityId)) && PlayersPatent.GetValue(communityId, level))
            playersLevel[i] = level;
        else
            playersLevel[i] = 0;
    }
}