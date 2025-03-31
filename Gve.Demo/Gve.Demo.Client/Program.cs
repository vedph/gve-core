using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Gve.Demo.Client;

internal class Program
{
    static async Task Main(string[] args)
    {
        WebAssemblyHostBuilder builder =
            WebAssemblyHostBuilder.CreateDefault(args);

        await builder.Build().RunAsync();
    }
}
