using HighlightBlazor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

namespace Gve.Demo.Client;

internal class Program
{
    static async Task Main(string[] args)
    {
        WebAssemblyHostBuilder builder =
            WebAssemblyHostBuilder.CreateDefault(args);

        // Radzen services
        builder.Services.AddRadzenComponents();

        // HighlightBlazor
        builder.Services.AddHighlight();

        await builder.Build().RunAsync();
    }
}
