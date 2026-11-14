using bClipboard.Extensions;
using Gve.Demo.Components;
using HighlightBlazor;
using Radzen;

namespace Gve.Demo;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // add services to the container
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        // Radzen services
        builder.Services.AddRadzenComponents();

        // HighlightBlazor
        builder.Services.AddHighlight();

        // bClipboard
        builder.Services.AddBClipboardService();

        WebApplication app = builder.Build();

        // configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

        app.Run();
    }
}
