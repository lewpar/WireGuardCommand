using WireGuardCommand.Configuration;
using WireGuardCommand.Services;

namespace WireGuardCommand;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureServices(builder);

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        ConfigureMiddleware(app);

        app.Run();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.Configuration.Bind("WireGuardCommand", new WGCConfig());

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
}