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

        await builder.Build().RunAsync();
    }
}
