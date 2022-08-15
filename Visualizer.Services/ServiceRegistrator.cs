using Microsoft.Extensions.DependencyInjection;
using Visualizer.Services.Ingestion;
using Visualizer.Services.Query;

namespace Visualizer.Services;

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
