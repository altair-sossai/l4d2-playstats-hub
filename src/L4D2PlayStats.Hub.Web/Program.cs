using L4D2PlayStats.Hub.Web.Infrastructure.Telemetry;
using System.Security.Claims;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;
using System.Text.Json.Serialization;
using AspNet.Security.OpenId.Steam;
using L4D2PlayStats.Hub.Core.DependencyInjection;
using L4D2PlayStats.Hub.Core.Modules.Players.Services;
using L4D2PlayStats.Hub.Web;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication;
using L4D2PlayStats.Hub.Web.Infrastructure.Avatars;
using L4D2PlayStats.Hub.Web.Infrastructure.Backups;
using L4D2PlayStats.Hub.Web.Infrastructure.Steam;
using L4D2PlayStats.Hub.Web.Infrastructure.Branding;
using L4D2PlayStats.Hub.Web.Infrastructure.Errors;
using L4D2PlayStats.Hub.Web.Infrastructure.Health;
using L4D2PlayStats.Hub.Web.Infrastructure.BackgroundServices;
using L4D2PlayStats.Hub.Web.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Infrastructure.Localization;
using L4D2PlayStats.Hub.Web.Infrastructure.Middlewares;
using L4D2PlayStats.Hub.Web.Infrastructure.Proxies;
using L4D2PlayStats.Hub.Web.Infrastructure.Skins;
using L4D2PlayStats.Hub.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;

if (HealthCheck.IsCommand(args))
    return await HealthCheck.RunAsync(args);

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", true, true);

builder.Services.AddHubTelemetry(builder.Configuration);
builder.Services.AddHubCore();
builder.Services.AddHubDatabase(builder.Configuration, builder.Environment);

builder.Services.AddLocalization();
builder.Services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();

builder.Services
    .AddControllersWithViews(options => options.Filters.Add<SetupRedirectFilter>())
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddResponseCompression(options => options.EnableForHttps = true);
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>()
    .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service", StringComparison.Ordinal) && !typeof(IHostedService).IsAssignableFrom(type)))
    .AsImplementedInterfaces()
    .WithScopedLifetime());
builder.Services.AddScoped<IAuthorizationHandler, AdministratorHandler>();
builder.Services.AddSingleton<AvatarStore>();
builder.Services.AddSingleton<AvatarQueue>();
builder.Services.AddSingleton<SteamProfileClient>();
builder.Services.AddSingleton<BrandingStore>();
builder.Services.AddSingleton<SkinThumbnailStore>();
builder.Services.AddSingleton<BackupLock>();
builder.Services.AddSingleton<IBackupStorageFactory, AzureBackupStorageFactory>();
builder.Services.AddHostedService<SeasonBackgroundService>();
builder.Services.AddHostedService<BackupBackgroundService>();
builder.Services.AddHostedService<AvatarBackgroundService>();
builder.Services.AddHostedService<PlayerConnectionCleanupBackgroundService>();
builder.Services.AddHostedService<SkinSeasonRefundBackgroundService>();
builder.Services.AddHostedService<SkinAuthorSyncBackgroundService>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = SteamAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(AppConsts.LoginCookieInDays);
        options.SlidingExpiration = true;
        options.AccessDeniedPath = "/";
    })
    .AddSteam();

builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy(AppConsts.AdministratorPolicy, policy => policy.RequireAuthenticatedUser().AddRequirements(new AdministratorRequirement()));

var trustedProxies = TrustedProxyList.Parse(builder.Configuration[AppConsts.TrustedProxiesSetting]);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    trustedProxies.ApplyTo(options);
});

var app = builder.Build();

if (trustedProxies.Invalid.Count > 0)
    app.Logger.LogWarning("Ignored invalid entries in {Setting}: {Entries}", AppConsts.TrustedProxiesSetting, string.Join(", ", trustedProxies.Invalid));

await app.InitializeHubAsync();

if (HubCommands.IsCommand(args))
    return await app.RunCommandAsync(args);

var cultures = Cultures.Supported();
var localization = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(CoreConsts.DefaultCulture),
    SupportedCultures = cultures,
    SupportedUICultures = cultures
};

localization.RequestCultureProviders.Add(new SiteCultureProvider());

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(new ExceptionHandlerOptions { ExceptionHandlingPath = AppConsts.ErrorPath, SuppressDiagnosticsCallback = _ => true });
    app.UseHsts();
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<InGameBrowserMiddleware>();

app.UseWhen(ErrorPages.ShowsPage, branch => branch.UseStatusCodePagesWithReExecute(AppConsts.ErrorStatusPath));
app.UseMiddleware<ErrorLoggingMiddleware>();
app.UseRequestLocalization(localization);
app.UseResponseCompression();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>(AppConsts.DevelopmentSignInSetting))
    app.MapGet("/dev/sign-in/{communityId:long}", async (long communityId, string? returnUrl, HttpContext httpContext) =>
    {
        var knownName = await httpContext.RequestServices.GetRequiredService<IPlayerNameService>().KnownNameAsync(communityId, httpContext.RequestAborted);
        var name = string.IsNullOrWhiteSpace(knownName) ? string.Format(AppConsts.DevelopmentSignInNameFormat, communityId) : knownName;
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, communityId.ToString()), new Claim(ClaimTypes.Name, name) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = true });

        return Results.Redirect(!string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith('/') && !returnUrl.StartsWith("//") ? returnUrl : "/");
    });

await app.RunAsync();

return 0;

public partial class Program;
