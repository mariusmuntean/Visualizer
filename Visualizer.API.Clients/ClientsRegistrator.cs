using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Visualizer.API.Clients;

public static class ClientsRegistrator
{
    public static void Register(WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Services.AddHttpClient<IIngestionClient>(client =>
        {
            var ingestionServiceUrl = webApplicationBuilder.Configuration.GetSection("Ingestion")["Url"];
            client.BaseAddress = new Uri(ingestionServiceUrl);
        });
        webApplicationBuilder.Services.AddSingleton<IIngestionClient, IngestionClient>();
    }
}
