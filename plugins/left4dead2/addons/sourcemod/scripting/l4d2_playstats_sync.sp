#pragma semicolon 1
#pragma newdecls required
#pragma dynamic 524288

#include <sourcemod>
#include <regex>
#include <l4d2_playstats_hub>

#define CONSOLE_PREFIX "[PlayStats]"
#define TEST_COMMAND "sm_playstats_test"
#define STATISTICS_PATH "/api/statistics"
#define STATISTICS_FOLDER "logs"
#define STATISTICS_FILE_PATTERN "^\\w{4}-\\w{2}-\\w{2}_\\w{2}-\\w{2}_\\d{4}.*\\.txt$"
#define STATISTICS_FILE_NAME_LENGTH 128
#define STATISTICS_MAX_FILE_SIZE 1048576
#define STATISTICS_SYNC_DELAY 10.0
#define STATISTICS_UPLOAD_TIMEOUT 60
#define STATISTICS_SYNC_TIMEOUT 600
#define STATISTICS_RESEND_UNCHANGED_AFTER 1800
#define MESSAGE_LENGTH 512
#define LIST_LENGTH 256

enum struct StatisticsAnswer
{
    int size;
    int changedAt;
    int answeredAt;
}

public Plugin myinfo =
{
    name        = "L4D2 - Player Statistics Sync",
    author      = "Altair Sossai",
    description = "Connects the game server to L4D2 PlayStats Hub and uploads the statistics files written by l4d2_playstats",
    version     = "2.0.1",
    url         = "https://github.com/altair-sossai/l4d2-playstats-hub"
};

ArrayList g_hQueue;
StringMap g_hAnswers;

bool
    g_bSyncScheduled = false,
    g_bHeartbeatScheduled = false,
    g_bHeartbeatPending = false,
    g_bHeartbeatRepeat = false,
    g_bHibernating = false,
    g_bUploadFailing = false;

PlayStatsHubResult g_iLastResult = PlayStatsHubResult_Unknown;

char g_sLastMessage[MESSAGE_LENGTH];

int
    g_iLastHeartbeatAt = 0,
    g_iSyncStartedAt = 0;

public void OnPluginStart()
{
    PlayStatsHub_CreateConVars();
    PlayStatsHub_HookChange(OnAddressChanged);

    g_hAnswers = new StringMap();

    RegAdminCmd(TEST_COMMAND, Command_Test, ADMFLAG_ROOT, "Checks the connection with the L4D2 PlayStats Hub site");

    HookEvent("round_start", Event_RoundStart, EventHookMode_PostNoCopy);

    CreateTimer(PLAYSTATS_HUB_HEARTBEAT_INTERVAL, Timer_Heartbeat, _, TIMER_REPEAT);
}

public void OnMapStart()
{
    g_bHibernating = false;
}

public void OnConfigsExecuted()
{
    SendHeartbeat();
}

public void OnServerEnterHibernation()
{
    g_bHibernating = true;
}

public void OnServerExitHibernation()
{
    g_bHibernating = false;

    SendHeartbeat();
}

void OnAddressChanged(ConVar convar, const char[] oldValue, const char[] newValue)
{
    g_iLastResult = PlayStatsHubResult_Unknown;

    if (g_bHeartbeatScheduled)
        return;

    g_bHeartbeatScheduled = true;

    RequestFrame(Frame_AddressChanged);
}

void Frame_AddressChanged()
{
    g_bHeartbeatScheduled = false;
    g_bHeartbeatRepeat = g_bHeartbeatPending;

    SendHeartbeat();
}

Action Timer_Heartbeat(Handle timer)
{
    if (!g_bHibernating)
        SendHeartbeat();

    return Plugin_Continue;
}

void SendHeartbeat()
{
    if (g_bHeartbeatPending)
        return;

    if (!PlayStatsHub_IsConfigured())
    {
        RememberResult(PlayStatsHubResult_UrlNotSet, HTTPStatus_Invalid, "", "", "", "");
        return;
    }

    char token[PLAYSTATS_HUB_TOKEN_LENGTH];
    if (!PlayStatsHub_GetToken(token, sizeof(token)))
    {
        RememberResult(PlayStatsHubResult_TokenNotSet, HTTPStatus_Invalid, "", "", "", "");
        return;
    }

    g_bHeartbeatPending = PlayStatsHub_SendHeartbeat(OnHeartbeatAnswered);
}

void OnHeartbeatAnswered(HTTPResponse response, any value, const char[] error)
{
    g_bHeartbeatPending = false;

    char url[PLAYSTATS_HUB_URL_LENGTH];
    PlayStatsHub_GetUrl(url, sizeof(url));

    char server[PLAYSTATS_HUB_NAME_LENGTH];
    char site[PLAYSTATS_HUB_NAME_LENGTH];

    PlayStatsHubResult result = PlayStatsHub_ReadHeartbeat(response);
    if (result == PlayStatsHubResult_Connected)
    {
        PlayStatsHub_ReadString(response, "server", server, sizeof(server));
        PlayStatsHub_ReadString(response, "site", site, sizeof(site));
    }

    RememberResult(result, response.Status, url, error, server, site);

    if (!g_bHeartbeatRepeat)
        return;

    g_bHeartbeatRepeat = false;

    SendHeartbeat();
}

void RememberResult(PlayStatsHubResult result, HTTPStatus status, const char[] url, const char[] error, const char[] server, const char[] site)
{
    DescribeResult(result, status, url, error, server, site, g_sLastMessage, sizeof(g_sLastMessage));

    g_iLastHeartbeatAt = GetTime();

    if (result == g_iLastResult)
        return;

    g_iLastResult = result;

    if (result == PlayStatsHubResult_Connected)
        LogMessage("%s", g_sLastMessage);
    else
        LogError("%s", g_sLastMessage);
}

void DescribeResult(PlayStatsHubResult result, HTTPStatus status, const char[] url, const char[] error, const char[] server, const char[] site, char[] message, int maxlength)
{
    switch (result)
    {
        case PlayStatsHubResult_Connected:
            FormatEx(message, maxlength, "Connected to the site \"%s\" as the game server \"%s\".", site, server);
        case PlayStatsHubResult_UrlNotSet:
            FormatEx(message, maxlength, "URL not set: %s is empty. Set it to the address of the site, for example %s \"https://example.com\".", PLAYSTATS_HUB_URL_CVAR, PLAYSTATS_HUB_URL_CVAR);
        case PlayStatsHubResult_TokenNotSet:
            FormatEx(message, maxlength, "Token not set: %s is empty. Set it to the token the site shows for this game server, in the form <identifier>:<key>.", PLAYSTATS_HUB_TOKEN_CVAR);
        case PlayStatsHubResult_Unreachable:
            FormatEx(message, maxlength, "Cannot reach %s (%s). Check %s, the network and whether the site is running.", url, error, PLAYSTATS_HUB_URL_CVAR);
        case PlayStatsHubResult_UnknownServer:
            FormatEx(message, maxlength, "Unknown identifier: the site has no game server with the identifier in %s. Copy the token from the page of this game server on the site.", PLAYSTATS_HUB_TOKEN_CVAR);
        case PlayStatsHubResult_InvalidKey:
            FormatEx(message, maxlength, "Wrong key: the site knows the identifier in %s, but the key does not match. Generate a new key on the site and update the token.", PLAYSTATS_HUB_TOKEN_CVAR);
        case PlayStatsHubResult_MissingToken:
            FormatEx(message, maxlength, "Token not accepted: %s must have the form <identifier>:<key>.", PLAYSTATS_HUB_TOKEN_CVAR);
        case PlayStatsHubResult_Disabled:
            FormatEx(message, maxlength, "This game server is disabled on the site.");
        default:
            FormatEx(message, maxlength, "%s answered HTTP %d, which is not an answer of L4D2 PlayStats Hub. Check %s.", url, status, PLAYSTATS_HUB_URL_CVAR);
    }
}

Action Command_Test(int client, int args)
{
    char message[MESSAGE_LENGTH];

    char url[PLAYSTATS_HUB_URL_LENGTH];
    if (!PlayStatsHub_GetUrl(url, sizeof(url)))
    {
        DescribeResult(PlayStatsHubResult_UrlNotSet, HTTPStatus_Invalid, "", "", "", "", message, sizeof(message));
        ReplyToCommand(client, "%s %s", CONSOLE_PREFIX, message);
        return Plugin_Handled;
    }

    char token[PLAYSTATS_HUB_TOKEN_LENGTH];
    if (!PlayStatsHub_GetToken(token, sizeof(token)))
    {
        DescribeResult(PlayStatsHubResult_TokenNotSet, HTTPStatus_Invalid, "", "", "", "", message, sizeof(message));
        ReplyToCommand(client, "%s %s", CONSOLE_PREFIX, message);
        return Plugin_Handled;
    }

    if (g_iLastHeartbeatAt > 0)
        ReplyToCommand(client, "%s Last heartbeat, %d seconds ago: %s", CONSOLE_PREFIX, GetTime() - g_iLastHeartbeatAt, g_sLastMessage);

    DataPack pack = new DataPack();
    pack.WriteCell(client == 0 ? 0 : GetClientUserId(client));
    pack.WriteCell(GetCmdReplySource());
    pack.WriteString(url);

    if (!PlayStatsHub_SendHeartbeat(OnTestAnswered, pack))
    {
        delete pack;
        return Plugin_Handled;
    }

    ReplyToCommand(client, "%s Calling %s now. The answer is printed here in a moment.", CONSOLE_PREFIX, url);

    return Plugin_Handled;
}

void OnTestAnswered(HTTPResponse response, DataPack pack, const char[] error)
{
    char url[PLAYSTATS_HUB_URL_LENGTH];

    pack.Reset();
    int userId = pack.ReadCell();
    ReplySource source = pack.ReadCell();
    pack.ReadString(url, sizeof(url));
    delete pack;

    int client = 0;
    if (userId != 0)
    {
        client = GetClientOfUserId(userId);
        if (client == 0)
            return;
    }

    char server[PLAYSTATS_HUB_NAME_LENGTH];
    char site[PLAYSTATS_HUB_NAME_LENGTH];
    char modules[LIST_LENGTH];

    PlayStatsHubResult result = PlayStatsHub_ReadHeartbeat(response);
    if (result == PlayStatsHubResult_Connected)
    {
        PlayStatsHub_ReadString(response, "server", server, sizeof(server));
        PlayStatsHub_ReadString(response, "site", site, sizeof(site));
        ReadModules(response, modules, sizeof(modules));
    }

    RememberResult(result, response.Status, url, error, server, site);

    ReplySource previous = SetCmdReplySource(source);

    ReplyToCommand(client, "%s %s", CONSOLE_PREFIX, g_sLastMessage);

    if (result == PlayStatsHubResult_Connected)
        ReplyToCommand(client, "%s Modules turned on: %s", CONSOLE_PREFIX, modules[0] == '\0' ? "none" : modules);

    ReplyLocalChecks(client);

    SetCmdReplySource(previous);
}

void ReadModules(HTTPResponse response, char[] modules, int maxlength)
{
    modules[0] = '\0';

    JSONObject data = view_as<JSONObject>(response.Data);
    if (data == null || !data.HasKey("modules"))
        return;

    JSONArray list = view_as<JSONArray>(data.Get("modules"));

    char module[PLAYSTATS_HUB_PLUGIN_LENGTH];
    for (int i = 0; i < list.Length; i++)
    {
        if (!list.GetString(i, module, sizeof(module)))
            continue;

        if (modules[0] != '\0')
            StrCat(modules, maxlength, ", ");

        StrCat(modules, maxlength, module);
    }

    delete list;
}

void ReplyLocalChecks(int client)
{
    char plugins[LIST_LENGTH];
    DescribePlugins(plugins, sizeof(plugins));

    ReplyToCommand(client, "%s Plugins loaded: %s", CONSOLE_PREFIX, plugins);

    if (PlayStatsHub_IsWritingStatistics())
        ReplyToCommand(client, "%s Statistics writing is on.", CONSOLE_PREFIX);
    else
        ReplyToCommand(client, "%s Statistics writing is off: load l4d2_playstats and set %s 1.", CONSOLE_PREFIX, PLAYSTATS_HUB_STATISTICS_WRITING_CVAR);

    ArrayList files = FindStatisticsFiles();
    ReplyToCommand(client, "%s Statistics files waiting to be sent: %d", CONSOLE_PREFIX, files.Length);
    delete files;
}

void DescribePlugins(char[] plugins, int maxlength)
{
    plugins[0] = '\0';

    JSONObject loaded = PlayStatsHub_BuildPlugins();
    JSONObjectKeys names = loaded.Keys();

    char name[PLAYSTATS_HUB_PLUGIN_LENGTH];
    char version[PLAYSTATS_HUB_VERSION_LENGTH];

    while (names.ReadKey(name, sizeof(name)))
    {
        loaded.GetString(name, version, sizeof(version));

        if (plugins[0] != '\0')
            StrCat(plugins, maxlength, ", ");

        StrCat(plugins, maxlength, name);
        StrCat(plugins, maxlength, " ");
        StrCat(plugins, maxlength, version);
    }

    delete names;
    delete loaded;
}

void Event_RoundStart(Event event, const char[] name, bool dontBroadcast)
{
    if (g_bSyncScheduled)
        return;

    g_bSyncScheduled = true;

    CreateTimer(STATISTICS_SYNC_DELAY, Timer_Sync);
}

Action Timer_Sync(Handle timer)
{
    g_bSyncScheduled = false;

    Sync();

    return Plugin_Stop;
}

void Sync()
{
    if (!PlayStatsHub_IsConfigured())
        return;

    if (g_hQueue != null && GetTime() - g_iSyncStartedAt < STATISTICS_SYNC_TIMEOUT)
        return;

    delete g_hQueue;

    ForgetMissingFiles();

    g_iSyncStartedAt = GetTime();
    g_hQueue = FindStatisticsFiles();

    UploadNext();
}

ArrayList FindStatisticsFiles()
{
    ArrayList files = new ArrayList(ByteCountToCells(STATISTICS_FILE_NAME_LENGTH));

    char folder[PLATFORM_MAX_PATH];
    BuildPath(Path_SM, folder, sizeof(folder), STATISTICS_FOLDER);

    DirectoryListing listing = OpenDirectory(folder);
    if (listing == null)
        return files;

    Regex pattern = new Regex(STATISTICS_FILE_PATTERN);
    if (pattern == null)
    {
        delete listing;
        return files;
    }

    char fileName[STATISTICS_FILE_NAME_LENGTH];
    FileType type;

    while (listing.GetNext(fileName, sizeof(fileName), type))
    {
        if (type == FileType_File && pattern.Match(fileName) > 0)
            files.PushString(fileName);
    }

    delete pattern;
    delete listing;

    files.Sort(Sort_Ascending, Sort_String);

    return files;
}

void ForgetMissingFiles()
{
    StringMapSnapshot snapshot = g_hAnswers.Snapshot();

    char fileName[STATISTICS_FILE_NAME_LENGTH];
    char path[PLATFORM_MAX_PATH];

    for (int i = 0; i < snapshot.Length; i++)
    {
        snapshot.GetKey(i, fileName, sizeof(fileName));
        BuildStatisticsPath(fileName, path, sizeof(path));

        if (!FileExists(path))
            g_hAnswers.Remove(fileName);
    }

    delete snapshot;
}

void BuildStatisticsPath(const char[] fileName, char[] path, int maxlength)
{
    BuildPath(Path_SM, path, maxlength, "%s/%s", STATISTICS_FOLDER, fileName);
}

void UploadNext()
{
    char fileName[STATISTICS_FILE_NAME_LENGTH];

    while (g_hQueue != null && g_hQueue.Length > 0)
    {
        g_hQueue.GetString(0, fileName, sizeof(fileName));
        g_hQueue.Erase(0);

        if (Upload(fileName))
            return;
    }

    delete g_hQueue;
}

bool Upload(const char[] fileName)
{
    char path[PLATFORM_MAX_PATH];
    BuildStatisticsPath(fileName, path, sizeof(path));

    int size = FileSize(path);
    if (size <= 0)
        return false;

    if (size > STATISTICS_MAX_FILE_SIZE)
    {
        LogError("%s has %d bytes, more than the %d bytes this plugin sends", fileName, size, STATISTICS_MAX_FILE_SIZE);
        return false;
    }

    int changedAt = GetFileTime(path, FileTime_LastChange);
    if (!NeedsUpload(fileName, size, changedAt))
        return false;

    char[] content = new char[size + 1];
    if (!ReadStatisticsFile(path, content, size))
    {
        LogError("Could not read %s", fileName);
        return false;
    }

    JSONObject command = new JSONObject();
    command.SetString("fileName", fileName);

    if (!command.SetString("content", content))
    {
        delete command;
        LogError("Could not prepare %s to be sent", fileName);
        return false;
    }

    HTTPRequest request = PlayStatsHub_CreateRequest(STATISTICS_PATH);
    if (request == null)
    {
        delete command;
        return false;
    }

    request.Timeout = STATISTICS_UPLOAD_TIMEOUT;

    DataPack pack = new DataPack();
    pack.WriteString(fileName);
    pack.WriteCell(size);
    pack.WriteCell(changedAt);

    request.Post(command, OnUploadAnswered, pack);

    delete command;

    return true;
}

bool NeedsUpload(const char[] fileName, int size, int changedAt)
{
    StatisticsAnswer answer;
    if (!g_hAnswers.GetArray(fileName, answer, sizeof(answer)))
        return true;

    if (answer.size != size || answer.changedAt != changedAt)
        return true;

    return GetTime() - answer.answeredAt >= STATISTICS_RESEND_UNCHANGED_AFTER;
}

bool ReadStatisticsFile(const char[] path, char[] content, int size)
{
    File file = OpenFile(path, "r");
    if (file == null)
        return false;

    int length = file.ReadString(content, size + 1, size);

    delete file;

    if (length <= 0)
        return false;

    content[length] = '\0';

    ReplaceInvalidUtf8(content, length);

    return true;
}

void ReplaceInvalidUtf8(char[] text, int length)
{
    int position = 0;

    while (position < length)
    {
        int sequence = Utf8SequenceLength(text, position, length);
        if (sequence == 0)
        {
            text[position] = '?';
            sequence = 1;
        }

        position += sequence;
    }
}

int Utf8SequenceLength(const char[] text, int position, int length)
{
    int first = text[position] & 0xFF;
    if (first < 0x80)
        return 1;

    int sequence = 0;
    int lowest = 0x80;
    int highest = 0xBF;

    if (first >= 0xC2 && first <= 0xDF)
        sequence = 2;
    else if (first >= 0xE0 && first <= 0xEF)
        sequence = 3;
    else if (first >= 0xF0 && first <= 0xF4)
        sequence = 4;
    else
        return 0;

    if (first == 0xE0)
        lowest = 0xA0;
    else if (first == 0xED)
        highest = 0x9F;
    else if (first == 0xF0)
        lowest = 0x90;
    else if (first == 0xF4)
        highest = 0x8F;

    if (position + sequence > length)
        return 0;

    int second = text[position + 1] & 0xFF;
    if (second < lowest || second > highest)
        return 0;

    for (int i = 2; i < sequence; i++)
    {
        int next = text[position + i] & 0xFF;
        if (next < 0x80 || next > 0xBF)
            return 0;
    }

    return sequence;
}

void OnUploadAnswered(HTTPResponse response, DataPack pack, const char[] error)
{
    char fileName[STATISTICS_FILE_NAME_LENGTH];

    pack.Reset();
    pack.ReadString(fileName, sizeof(fileName));
    int size = pack.ReadCell();
    int changedAt = pack.ReadCell();
    delete pack;

    if (response.Status == HTTPStatus_OK)
    {
        if (g_bUploadFailing)
        {
            g_bUploadFailing = false;
            LogMessage("Statistics files are being sent again.");
        }

        if (MustBeDeleted(response))
            DeleteStatisticsFile(fileName);
        else
            RememberAnswer(fileName, size, changedAt);

        UploadNext();
        return;
    }

    if (response.Status == HTTPStatus_BadRequest)
    {
        RememberAnswer(fileName, size, changedAt);
        UploadNext();
        return;
    }

    delete g_hQueue;

    if (g_bUploadFailing)
        return;

    g_bUploadFailing = true;

    if (response.Status == HTTPStatus_Invalid)
        LogError("Could not send %s: %s. The files stay on the server and are tried again at the next round start; this is logged once until a file is sent.", fileName, error);
    else
        LogError("Could not send %s (HTTP %d). The files stay on the server and are tried again at the next round start; this is logged once until a file is sent.", fileName, response.Status);
}

bool MustBeDeleted(HTTPResponse response)
{
    if (!PlayStatsHub_HasJson(response))
        return false;

    JSONObject data = view_as<JSONObject>(response.Data);

    return data != null && data.HasKey("mustBeDeleted") && data.GetBool("mustBeDeleted");
}

void DeleteStatisticsFile(const char[] fileName)
{
    char path[PLATFORM_MAX_PATH];
    BuildStatisticsPath(fileName, path, sizeof(path));

    if (!DeleteFile(path))
        LogError("Could not delete %s", fileName);

    g_hAnswers.Remove(fileName);
}

void RememberAnswer(const char[] fileName, int size, int changedAt)
{
    StatisticsAnswer answer;
    answer.size = size;
    answer.changedAt = changedAt;
    answer.answeredAt = GetTime();

    g_hAnswers.SetArray(fileName, answer, sizeof(answer));
}
