using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Visualizer.API.Clients.Impl;

namespace Visualizer.API.Clients;

public static class VisualizerClientsRegistrator
{
    public static void Register(WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Services.AddHttpClient<IIngestionClient, IngestionClient>((provider, client) =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var ingestionServiceUrl = config.GetSection("Ingestion")["Url"].TrimStart("https://".ToCharArray());
                ingestionServiceUrl = $"https://{ingestionServiceUrl}";
                provider.GetService<ILogger<IngestionClient>>()?.LogInformation("Ingestion service URL: {IngestionServiceUrl}", ingestionServiceUrl);
                client.BaseAddress = new Uri(ingestionServiceUrl);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetIngestionServicePolicy);
    }

    private static IAsyncPolicy<HttpResponseMessage> GetIngestionServicePolicy(HttpRequestMessage httpRequestMessage)
    {
        return HttpPolicyExtensions
            // Handle HttpRequestExceptions, 408 and 5xx status codes
            .HandleTransientHttpError()
            // Handle 404 not found
            .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
            // Handle 401 Unauthorized
            .OrResult(msg => msg.StatusCode == HttpStatusCode.Unauthorized)
            // What to do if any of the above errors occur:
            // Retry 3 times, each time wait 1,2 and 4 seconds before retrying.
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
