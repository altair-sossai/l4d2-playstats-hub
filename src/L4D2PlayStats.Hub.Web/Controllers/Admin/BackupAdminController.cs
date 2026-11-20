using L4D2PlayStats.Hub.Core.Modules.Backups.Enums;
using L4D2PlayStats.Hub.Core.Modules.Backups.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Backups.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Web.Models.Backups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using L4D2PlayStats.Hub.Core.Modules.Backups.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Backups.Contracts;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[Authorize(Policy = AppConsts.AdministratorPolicy)]
[AutoValidateAntiforgeryToken]
[Route("admin/backup")]
public class BackupAdminController(
    ISettingsService settingsService,
    IBackupHistoryService historyService,
    IBackupService backupService,
    IBackupArchiveService archiveService,
    IBackupStorageFactory storageFactory,
    ILiveViewService liveViewService,
    SecretsKey secretsKey) : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewBag.Admin = "active";
        ViewBag.Administration = "active";
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var settings = await settingsService.GetAsync<BackupSettings>(cancellationToken);
        var history = await historyService.GetAsync(cancellationToken);
        var timeZone = await liveViewService.TimeZoneAsync(cancellationToken);

        return View(new BackupAdminModel(settings, history, archiveService.IncludesDatabase, backupService.IsRunning, timeZone, secretsKey));
    }

    [HttpPost("")]
    public async Task<IActionResult> Save(BackupForm form, CancellationToken cancellationToken)
    {
        var settings = await settingsService.GetAsync<BackupSettings>(cancellationToken);
        var problems = new List<string>();

        settings.Enabled = form.Enabled;
        settings.Destination = Enum.IsDefined(form.Destination) ? form.Destination : BackupDestination.ContainerUrl;
        settings.IntervalInHours = BackupRules.Interval(form.IntervalInHours);
        settings.Keep = BackupRules.Keep(form.Keep);

        ApplyContainerUrl(settings, form.ContainerUrl, problems);
        ApplyConnectionString(settings, form.ConnectionString, problems);
        ApplyContainerName(settings, form.ContainerName, problems);
        ApplyPrefix(settings, form.Prefix, problems);

        await settingsService.SaveAsync(settings, cancellationToken);

        if (problems.Count > 0)
            TempData[AppConsts.SettingsProblemsTempData] = problems.ToArray();
        else
            TempData["Saved"] = true;

        return Redirect(AppConsts.AdminBackupUrl);
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run(CancellationToken cancellationToken)
    {
        var run = await backupService.RunAsync(BackupTrigger.Manual, cancellationToken);

        if (run == null)
            Report(AppConsts.BackupErrorTempData, AppConsts.BackupBusyMessage, null);
        else if (run.Succeeded)
            Report(AppConsts.BackupMessageTempData, AppConsts.BackupDoneMessage, run.BlobName);
        else
            Report(AppConsts.BackupErrorTempData, AppConsts.BackupFailedMessage, run.Error);

        return Redirect(AppConsts.AdminBackupUrl);
    }

    [HttpPost("test")]
    public async Task<IActionResult> Test(CancellationToken cancellationToken)
    {
        var check = await backupService.TestAsync(cancellationToken);

        if (check.Succeeded)
            Report(AppConsts.BackupMessageTempData, AppConsts.BackupTestPassedMessage, null);
        else
            Report(AppConsts.BackupErrorTempData, AppConsts.BackupTestFailedMessage, check.Error);

        return Redirect(AppConsts.AdminBackupUrl);
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify(CancellationToken cancellationToken)
    {
        var result = await backupService.VerifyLatestAsync(cancellationToken);

        if (result == null)
        {
            Report(AppConsts.BackupErrorTempData, AppConsts.BackupBusyMessage, null);

            return Redirect(AppConsts.AdminBackupUrl);
        }

        TempData[AppConsts.BackupVerifyStatusTempData] = result.Verification.Status.ToString();
        TempData[AppConsts.BackupVerifyMessageTempData] = result.Verification.Message;
        TempData[AppConsts.BackupVerifyArgumentsTempData] = result.Verification.Arguments.ToArray();
        TempData[AppConsts.BackupVerifyBlobTempData] = result.BlobName;

        return Redirect(AppConsts.AdminBackupUrl);
    }

    private void Report(string kind, string message, string? detail)
    {
        TempData[kind] = message;
        TempData[AppConsts.BackupDetailTempData] = detail;
    }

    private static void ApplyContainerUrl(BackupSettings settings, string? containerUrl, List<string> problems)
    {
        if (string.IsNullOrWhiteSpace(containerUrl))
            return;

        if (BackupRules.IsValidContainerUrl(containerUrl))
            settings.ContainerUrl = containerUrl.Trim();
        else
            problems.Add(AppConsts.BackupContainerUrlIgnoredMessage);
    }

    private void ApplyConnectionString(BackupSettings settings, string? connectionString, List<string> problems)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        if (storageFactory.IsValidConnectionString(connectionString))
            settings.ConnectionString = connectionString.Trim();
        else
            problems.Add(AppConsts.BackupConnectionStringIgnoredMessage);
    }

    private static void ApplyContainerName(BackupSettings settings, string? containerName, List<string> problems)
    {
        var name = containerName?.Trim();

        if (string.IsNullOrEmpty(name))
            settings.ContainerName = null;
        else if (BackupRules.IsValidContainerName(name))
            settings.ContainerName = name;
        else
            problems.Add(AppConsts.BackupContainerNameIgnoredMessage);
    }

    private static void ApplyPrefix(BackupSettings settings, string? prefix, List<string> problems)
    {
        var trimmed = prefix?.Trim();

        if (string.IsNullOrEmpty(trimmed))
            settings.Prefix = CoreConsts.BackupDefaultPrefix;
        else if (BackupRules.IsValidPrefix(trimmed))
            settings.Prefix = trimmed;
        else
            problems.Add(AppConsts.BackupPrefixIgnoredMessage);
    }
}
