#pragma semicolon 1
#pragma newdecls required

#include <sourcemod>
#include <left4dhooks>
#include <colors>
#include <l4d2_playstats_hub>

#define INGAME_URL_CVAR "playstats_ingame_url"
#define INGAME_PATH "/ingame"
#define INGAME_URL_LENGTH 384
#define SCHEME_SEPARATOR "://"
#define STEAM_ID_LENGTH 32
#define TITLE_LENGTH 64
#define HINT_INTERVAL 200.0
#define AUTOMATIC_OPENING_DELAY 60.0
#define NEW_GAME_CHECK_DELAY 10.0

public Plugin myinfo =
{
    name        = "L4D2 - Ranking",
    author      = "Altair Sossai",
    description = "Opens the in-game ranking page of L4D2 PlayStats Hub",
    version     = "2.0.0",
    url         = "https://github.com/altair-sossai/l4d2-playstats-hub"
};

ConVar
    hInGameUrl,
    hShowMOTDPanel;

bool showRankingEnabled = true;
bool displayed[MAXPLAYERS + 1] = { false, ... };

public void OnPluginStart()
{
    LoadTranslations("l4d2_ranking.phrases");

    PlayStatsHub_CreateConVars();

    hInGameUrl = CreateConVar(INGAME_URL_CVAR, "", "Address the game opens for the in-game pages, without a trailing slash. When empty, playstats_url is used");
    hShowMOTDPanel = CreateConVar("ranking_show_motd_panel", "1", "Show MOTD panel with the ranking site URL", FCVAR_NOTIFY, true, 0.0, true, 1.0);

    RegConsoleCmd("sm_ranking", ShowRankingCmd);

    HookEvent("round_start", RoundStart_Event, EventHookMode_PostNoCopy);

    CreateTimer(HINT_INTERVAL, DisplayRankingUrlTick, _, TIMER_REPEAT);
}

public void OnRoundIsLive()
{
    showRankingEnabled = false;
    ResetRankingDisplayed();
}

public void OnClientPutInServer(int client)
{
    displayed[client] = false;

    if (!IsFakeClient(client))
        CreateTimer(AUTOMATIC_OPENING_DELAY, ShowRankingTick, GetClientUserId(client));
}

void RoundStart_Event(Event hEvent, const char[] eName, bool dontBroadcast)
{
    CreateTimer(NEW_GAME_CHECK_DELAY, NewGameTick);
}

Action NewGameTick(Handle timer)
{
    showRankingEnabled = IsNewGame();

    return Plugin_Stop;
}

Action ShowRankingCmd(int client, int args)
{
    if (client > 0)
        ShowRanking(client);

    return Plugin_Handled;
}

Action DisplayRankingUrlTick(Handle timer)
{
    if (!showRankingEnabled)
        return Plugin_Continue;

    char site[PLAYSTATS_HUB_URL_LENGTH];
    if (GetSiteAddress(site, sizeof(site)))
        CPrintToChatAll("%t", "RankingWebsite", site);

    CPrintToChatAll("%t", "RankingCommandHint");

    return Plugin_Continue;
}

Action ShowRankingTick(Handle timer, int userId)
{
    int client = GetClientOfUserId(userId);

    if (client > 0 && showRankingEnabled && !displayed[client])
        ShowRanking(client);

    return Plugin_Stop;
}

void ShowRanking(int client)
{
    if (!IsClientInGame(client) || IsFakeClient(client) || !hShowMOTDPanel.IntValue)
        return;

    char url[INGAME_URL_LENGTH];
    if (!GetInGameUrl(client, url, sizeof(url)))
        return;

    displayed[client] = true;

    char title[TITLE_LENGTH];
    FormatEx(title, sizeof(title), "%T", "RankingPanelTitle", client);

    ShowMOTDPanel(client, title, url, MOTDPANEL_TYPE_URL);
}

bool GetInGameUrl(int client, char[] url, int maxlength)
{
    char base[PLAYSTATS_HUB_URL_LENGTH];
    if (!GetInGameBase(base, sizeof(base)))
        return false;

    char steamId[STEAM_ID_LENGTH];
    if (GetClientAuthId(client, AuthId_SteamID64, steamId, sizeof(steamId)))
        FormatEx(url, maxlength, "%s%s?steamid=%s", base, INGAME_PATH, steamId);
    else
        FormatEx(url, maxlength, "%s%s", base, INGAME_PATH);

    return true;
}

bool GetInGameBase(char[] base, int maxlength)
{
    hInGameUrl.GetString(base, maxlength);
    TrimString(base);

    int length = strlen(base);
    while (length > 0 && base[length - 1] == '/')
    {
        length--;
        base[length] = '\0';
    }

    if (length > 0)
        return true;

    return PlayStatsHub_GetUrl(base, maxlength);
}

bool GetSiteAddress(char[] site, int maxlength)
{
    char url[PLAYSTATS_HUB_URL_LENGTH];
    if (!PlayStatsHub_GetUrl(url, sizeof(url)))
        return false;

    int scheme = StrContains(url, SCHEME_SEPARATOR);
    int start = scheme == -1 ? 0 : scheme + strlen(SCHEME_SEPARATOR);

    strcopy(site, maxlength, url[start]);

    return site[0] != '\0';
}

bool IsNewGame()
{
    return L4D2Direct_GetVSCampaignScore(0) == 0 && L4D2Direct_GetVSCampaignScore(1) == 0;
}

void ResetRankingDisplayed()
{
    for (int i = 0; i <= MaxClients; i++)
        displayed[i] = false;
}
