using ElectronNET.API.Entities;
using ElectronNET.API;

using WireGuardCommand.Configuration;
using WireGuardCommand.Services;
using System.Text.Json;

namespace WireGuardCommand;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        if(!builder.Environment.IsDevelopment())
        {
            builder.WebHost.UseElectron(args);
            builder.WebHost.UseStaticWebAssets();
        }

        await ConfigureServicesAsync(builder);

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        ConfigureMiddleware(app);

        await app.StartAsync();

        if(!app.Environment.IsDevelopment())
        {
            await ConfigureElectronAsync();
        }

        await app.WaitForShutdownAsync();
    }

    private static async Task ConfigureServicesAsync(WebApplicationBuilder builder)
    {
        var services = builder.Services;

        await LoadConfigurationAsync(services);

        services.AddSingleton<ProjectCache>();
        services.AddSingleton<ProjectManager>();

        services.AddSingleton<AlertController>();

        services.AddRazorPages();
        services.AddServerSideBlazor();
    }

    private static async Task LoadConfigurationAsync(IServiceCollection services)
    {
        var config = new WGCConfig();
        var path = Path.Combine("./", "wgc.json");

        try
        {

            if (!File.Exists(path))
            {
                await config.SaveAsync();
            }

            using (var fs = File.OpenRead(path))
            {
                config = await JsonSerializer.DeserializeAsync<WGCConfig>(fs);
            }

            if(config is null)
            {
                throw new Exception("Failed to load configuration.");
            }

            services.AddSingleton(config);
        }
        catch
        {
            services.AddSingleton(new WGCConfig());
        }
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