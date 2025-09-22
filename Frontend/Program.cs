using Frontend.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Frontend;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");
        
        string apiEndpoint;
        if (builder.HostEnvironment.Environment == "Development")
        {
            apiEndpoint = "https://localhost:5273/api/"; // Development endpoint
        }
        else
        {
            throw new Exception("Environment is not set to Development");
        }
        Console.WriteLine($"API Endpoint: {apiEndpoint}");

        // Registers HttpClient to API service with a configurable endpoint
        builder.Services.AddHttpClient<ApiService>(client =>
        {
            client.BaseAddress = new Uri(apiEndpoint);
            Console.WriteLine($"APIService BaseAddress: {client.BaseAddress}");
        });

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        await builder.Build().RunAsync();
    }
}