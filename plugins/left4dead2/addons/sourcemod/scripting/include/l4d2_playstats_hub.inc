#if defined _l4d2_playstats_hub_included_
    #endinput
#endif
#define _l4d2_playstats_hub_included_

#include <sourcemod>
#include <ripext>

#define PLAYSTATS_HUB_URL_CVAR "playstats_url"
#define PLAYSTATS_HUB_TOKEN_CVAR "playstats_token"
#define PLAYSTATS_HUB_PUBLIC_URL_CVAR "playstats_public_url"
#define PLAYSTATS_HUB_AUTHORIZATION_HEADER "Authorization"
#define PLAYSTATS_HUB_CONTENT_TYPE_HEADER "Content-Type"
#define PLAYSTATS_HUB_CONTENT_TYPE_LOWERCASE_HEADER "content-type"
#define PLAYSTATS_HUB_JSON_CONTENT_TYPE "json"
#define PLAYSTATS_HUB_HEARTBEAT_PATH "/api/servers/heartbeat"
#define PLAYSTATS_HUB_HEARTBEAT_INTERVAL 30.0
#define PLAYSTATS_HUB_HOSTNAME_CVAR "hostname"
#define PLAYSTATS_HUB_CONFIGURATION_CVAR "l4d_ready_cfg_name"
#define PLAYSTATS_HUB_SOURCEMOD_VERSION_CVAR "sourcemod_version"
#define PLAYSTATS_HUB_STATISTICS_WRITING_CVAR "sm_stats_writestats"
#define PLAYSTATS_HUB_PLUGIN_EXTENSION ".smx"
#define PLAYSTATS_HUB_URL_LENGTH 256
#define PLAYSTATS_HUB_TOKEN_LENGTH 256
#define PLAYSTATS_HUB_ENDPOINT_LENGTH 512
#define PLAYSTATS_HUB_NAME_LENGTH 128
#define PLAYSTATS_HUB_PLUGIN_LENGTH 64
#define PLAYSTATS_HUB_VERSION_LENGTH 32
#define PLAYSTATS_HUB_ERROR_LENGTH 32
#define PLAYSTATS_HUB_CONTENT_TYPE_LENGTH 64

enum PlayStatsHubResult
{
    PlayStatsHubResult_Unknown = 0,
    PlayStatsHubResult_Connected,
    PlayStatsHubResult_UrlNotSet,
    PlayStatsHubResult_TokenNotSet,
    PlayStatsHubResult_Unreachable,
    PlayStatsHubResult_UnknownServer,
    PlayStatsHubResult_InvalidKey,
    PlayStatsHubResult_MissingToken,
    PlayStatsHubResult_Disabled,
    PlayStatsHubResult_UnexpectedAnswer
};

static const char g_sPlayStatsHubPlugins[][] =
{
    "l4d2_playstats_sync",
    "l4d2_gameinfo_sync",
    "l4d2_ranking",
    "l4d2_show_patent_icon",
    "l4d2_weapon_skins"
};

static ConVar g_hPlayStatsHubUrl;
static ConVar g_hPlayStatsHubToken;
static ConVar g_hPlayStatsHubPublicUrl;

stock void PlayStatsHub_CreateConVars()
{
    g_hPlayStatsHubUrl = CreateConVar(PLAYSTATS_HUB_URL_CVAR, "", "Address of the L4D2 PlayStats Hub site, without a trailing slash");
    g_hPlayStatsHubToken = CreateConVar(PLAYSTATS_HUB_TOKEN_CVAR, "", "Token of this game server on the L4D2 PlayStats Hub site, in the form <identifier>:<key>", FCVAR_PROTECTED);
    g_hPlayStatsHubPublicUrl = CreateConVar(PLAYSTATS_HUB_PUBLIC_URL_CVAR, "", "Address of the site shown to players in chat, without a trailing slash. When empty, playstats_url is used");
}

stock void PlayStatsHub_HookChange(ConVarChanged callback)
{
    g_hPlayStatsHubUrl.AddChangeHook(callback);
    g_hPlayStatsHubToken.AddChangeHook(callback);
}

stock bool PlayStatsHub_TrimAddress(char[] url)
{
    TrimString(url);

    int length = strlen(url);
    while (length > 0 && url[length - 1] == '/')
    {
        length--;
        url[length] = '\0';
    }

    return length > 0;
}

stock bool PlayStatsHub_GetUrl(char[] url, int maxlength)
{
    g_hPlayStatsHubUrl.GetString(url, maxlength);

    return PlayStatsHub_TrimAddress(url);
}

stock bool PlayStatsHub_GetPublicUrl(char[] url, int maxlength)
{
    g_hPlayStatsHubPublicUrl.GetString(url, maxlength);

    if (PlayStatsHub_TrimAddress(url))
        return true;

    return PlayStatsHub_GetUrl(url, maxlength);
}

stock bool PlayStatsHub_GetToken(char[] token, int maxlength)
{
    g_hPlayStatsHubToken.GetString(token, maxlength);
    TrimString(token);

    return token[0] != '\0';
}

stock bool PlayStatsHub_IsConfigured()
{
    char url[PLAYSTATS_HUB_URL_LENGTH];

    return PlayStatsHub_GetUrl(url, sizeof(url));
}

stock HTTPRequest PlayStatsHub_CreateRequest(const char[] path)
{
    char url[PLAYSTATS_HUB_URL_LENGTH];
    if (!PlayStatsHub_GetUrl(url, sizeof(url)))
        return null;

    char endpoint[PLAYSTATS_HUB_ENDPOINT_LENGTH];
    FormatEx(endpoint, sizeof(endpoint), "%s%s", url, path);

    HTTPRequest request = new HTTPRequest(endpoint);

    char token[PLAYSTATS_HUB_TOKEN_LENGTH];
    if (PlayStatsHub_GetToken(token, sizeof(token)))
        request.SetHeader(PLAYSTATS_HUB_AUTHORIZATION_HEADER, "%s", token);

    return request;
}

stock bool PlayStatsHub_HasJson(HTTPResponse response)
{
    char contentType[PLAYSTATS_HUB_CONTENT_TYPE_LENGTH];

    bool found = response.GetHeader(PLAYSTATS_HUB_CONTENT_TYPE_HEADER, contentType, sizeof(contentType))
        || response.GetHeader(PLAYSTATS_HUB_CONTENT_TYPE_LOWERCASE_HEADER, contentType, sizeof(contentType));

    return found && StrContains(contentType, PLAYSTATS_HUB_JSON_CONTENT_TYPE, false) != -1;
}

stock bool PlayStatsHub_ReadString(HTTPResponse response, const char[] key, char[] buffer, int maxlength)
{
    buffer[0] = '\0';

    if (!PlayStatsHub_HasJson(response))
        return false;

    JSONObject data = view_as<JSONObject>(response.Data);
    if (data == null)
        return false;

    return data.GetString(key, buffer, maxlength);
}

stock bool PlayStatsHub_IsWritingStatistics()
{
    ConVar writing = FindConVar(PLAYSTATS_HUB_STATISTICS_WRITING_CVAR);

    return writing != null && writing.IntValue > 0;
}

stock bool PlayStatsHub_IsHubPlugin(const char[] name)
{
    for (int i = 0; i < sizeof(g_sPlayStatsHubPlugins); i++)
    {
        if (StrEqual(name, g_sPlayStatsHubPlugins[i], false))
            return true;
    }

    return false;
}

stock void PlayStatsHub_GetPluginName(Handle plugin, char[] name, int maxlength)
{
    char file[PLATFORM_MAX_PATH];
    GetPluginFilename(plugin, file, sizeof(file));

    int start = 0;
    for (int i = 0; file[i] != '\0'; i++)
    {
        if (file[i] == '/' || file[i] == '\\')
            start = i + 1;
    }

    strcopy(name, maxlength, file[start]);

    int extension = strlen(name) - strlen(PLAYSTATS_HUB_PLUGIN_EXTENSION);
    if (extension > 0 && StrEqual(name[extension], PLAYSTATS_HUB_PLUGIN_EXTENSION, false))
        name[extension] = '\0';
}

stock JSONObject PlayStatsHub_BuildPlugins()
{
    JSONObject plugins = new JSONObject();
    Handle iterator = GetPluginIterator();

    char name[PLAYSTATS_HUB_PLUGIN_LENGTH];
    char version[PLAYSTATS_HUB_VERSION_LENGTH];

    while (MorePlugins(iterator))
    {
        Handle plugin = ReadPlugin(iterator);
        if (GetPluginStatus(plugin) != Plugin_Running)
            continue;

        PlayStatsHub_GetPluginName(plugin, name, sizeof(name));
        if (!PlayStatsHub_IsHubPlugin(name))
            continue;

        version[0] = '\0';
        GetPluginInfo(plugin, PlInfo_Version, version, sizeof(version));
        plugins.SetString(name, version);
    }

    delete iterator;

    return plugins;
}

stock void PlayStatsHub_SetConVarString(JSONObject target, const char[] key, const char[] cvar)
{
    char value[PLAYSTATS_HUB_NAME_LENGTH];

    ConVar source = FindConVar(cvar);
    if (source != null)
        source.GetString(value, sizeof(value));

    target.SetString(key, value);
}

stock JSONObject PlayStatsHub_BuildHeartbeat()
{
    JSONObject heartbeat = new JSONObject();

    PlayStatsHub_SetConVarString(heartbeat, "hostname", PLAYSTATS_HUB_HOSTNAME_CVAR);

    char map[PLATFORM_MAX_PATH];
    GetCurrentMap(map, sizeof(map));
    heartbeat.SetString("map", map);

    PlayStatsHub_SetConVarString(heartbeat, "configuration", PLAYSTATS_HUB_CONFIGURATION_CVAR);
    PlayStatsHub_SetConVarString(heartbeat, "sourceModVersion", PLAYSTATS_HUB_SOURCEMOD_VERSION_CVAR);

    heartbeat.SetInt("maxPlayers", GetMaxHumanPlayers());

    heartbeat.SetBool("statisticsWriting", PlayStatsHub_IsWritingStatistics());

    JSONObject plugins = PlayStatsHub_BuildPlugins();
    heartbeat.Set("plugins", plugins);
    delete plugins;

    return heartbeat;
}

stock bool PlayStatsHub_SendHeartbeat(HTTPRequestCallback callback, any value = 0)
{
    HTTPRequest request = PlayStatsHub_CreateRequest(PLAYSTATS_HUB_HEARTBEAT_PATH);
    if (request == null)
        return false;

    JSONObject heartbeat = PlayStatsHub_BuildHeartbeat();
    request.Post(heartbeat, callback, value);
    delete heartbeat;

    return true;
}

stock PlayStatsHubResult PlayStatsHub_ReadHeartbeat(HTTPResponse response)
{
    if (response.Status == HTTPStatus_Invalid)
        return PlayStatsHubResult_Unreachable;

    if (response.Status == HTTPStatus_OK)
    {
        char server[PLAYSTATS_HUB_NAME_LENGTH];
        if (PlayStatsHub_ReadString(response, "server", server, sizeof(server)))
            return PlayStatsHubResult_Connected;

        return PlayStatsHubResult_UnexpectedAnswer;
    }

    if (response.Status != HTTPStatus_Unauthorized)
        return PlayStatsHubResult_UnexpectedAnswer;

    char error[PLAYSTATS_HUB_ERROR_LENGTH];
    PlayStatsHub_ReadString(response, "error", error, sizeof(error));

    if (StrEqual(error, "UnknownServer"))
        return PlayStatsHubResult_UnknownServer;

    if (StrEqual(error, "InvalidKey"))
        return PlayStatsHubResult_InvalidKey;

    if (StrEqual(error, "MissingToken"))
        return PlayStatsHubResult_MissingToken;

    if (StrEqual(error, "Disabled"))
        return PlayStatsHubResult_Disabled;

    return PlayStatsHubResult_UnexpectedAnswer;
}
