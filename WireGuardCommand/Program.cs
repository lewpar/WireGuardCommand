using ElectronNET.API.Entities;
using ElectronNET.API;

using WireGuardCommand.Configuration;
using WireGuardCommand.Services;

namespace WireGuardCommand;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //builder.WebHost.UseElectron(args);
        //builder.WebHost.UseStaticWebAssets();

        ConfigureServices(builder);

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        ConfigureMiddleware(app);

        await app.StartAsync();

        //await ConfigureElectronAsync();

        await app.WaitForShutdownAsync();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.Configuration.Bind(WGCConfig.AppSettingsKey, new WGCConfig());

        services.AddSingleton<ProjectCache>();
        services.AddSingleton<ProjectManager>();

        services.AddRazorPages();
        services.AddServerSideBlazor();
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        app.UseStaticFiles();

        app.UseRouting();

        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");
    }

    private static async Task ConfigureElectronAsync()
    {
        var windowOptions = new BrowserWindowOptions
        {
            WebPreferences = new WebPreferences()
            {
                EnableRemoteModule = true,
                WebSecurity = true,
                ContextIsolation = true,
            },
            Width = 800,
            Height = 450,
            AutoHideMenuBar = true
        };

        await Electron.WindowManager.CreateWindowAsync(windowOptions);
    }
}