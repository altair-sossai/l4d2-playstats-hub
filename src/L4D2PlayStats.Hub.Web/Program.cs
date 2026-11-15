using System.Security.Claims;
using System.Text.Json.Serialization;
using AspNet.Security.OpenId.Steam;
using L4D2PlayStats.Hub.Core.DependencyInjection;
using L4D2PlayStats.Hub.Web;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication;
using L4D2PlayStats.Hub.Web.Infrastructure.Avatars;
using L4D2PlayStats.Hub.Web.Infrastructure.Branding;
using L4D2PlayStats.Hub.Web.Infrastructure.BackgroundServices;
using L4D2PlayStats.Hub.Web.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Infrastructure.Localization;
using L4D2PlayStats.Hub.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", true, true);

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
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>()
    .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service", StringComparison.Ordinal) && !typeof(IHostedService).IsAssignableFrom(type)))
    .AsImplementedInterfaces()
    .WithScopedLifetime());
builder.Services.AddScoped<IAuthorizationHandler, AdministratorHandler>();
builder.Services.AddSingleton<AvatarStore>();
builder.Services.AddSingleton<AvatarQueue>();
builder.Services.AddSingleton<BrandingStore>();
builder.Services.AddHostedService<SeasonBackgroundService>();
builder.Services.AddHostedService<AvatarBackgroundService>();
builder.Services.AddHostedService<PlayerConnectionCleanupBackgroundService>();
builder.Services.AddHostedService<SkinSeasonRefundBackgroundService>();

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

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

await app.InitializeHubAsync();

if (HubCommands.IsCommand(args))
    return await app.RunCommandAsync(args);

var cultures = Cultures.Supported();
var localization = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(AppConsts.DefaultCulture),
    SupportedCultures = cultures,
    SupportedUICultures = cultures
};

localization.RequestCultureProviders.Add(new SiteCultureProvider());

app.UseForwardedHeaders();
app.UseRequestLocalization(localization);
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>(AppConsts.DevelopmentSignInSetting))
    app.MapGet("/dev/sign-in/{communityId:long}", async (long communityId, string? returnUrl, HttpContext httpContext) =>
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, communityId.ToString()), new Claim(ClaimTypes.Name, $"Player {communityId}") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = true });

        return Results.Redirect(!string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith('/') && !returnUrl.StartsWith("//") ? returnUrl : "/");
    });

await app.RunAsync();

return 0;

public partial class Program;
