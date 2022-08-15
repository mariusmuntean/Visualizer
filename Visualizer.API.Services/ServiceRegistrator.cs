using Microsoft.Extensions.DependencyInjection;
using Visualizer.API.Services.Query;

namespace Visualizer.API.Services;

public static class ServiceRegistrator
{
    public static void Register(IServiceCollection serviceCollection)
    {
        // // Ingestion
        // serviceCollection.AddScoped<TwitterStreamService>();
        // serviceCollection.AddScoped<TweetGraphService>();
        // serviceCollection.AddScoped<TweetDbService>();
        // serviceCollection.AddSingleton<TweetHashtagService>();

        // Query
        serviceCollection.AddScoped<TweetDbQueryService>();
    }
}
