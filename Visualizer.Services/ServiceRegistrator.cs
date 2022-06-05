using Microsoft.Extensions.DependencyInjection;
using Visualizer.Services.Ingestion;

namespace Visualizer.Services;

public static class ServiceRegistrator
{
    public static void Register(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<TwitterStreamService>();
        serviceCollection.AddScoped<TweetGraphService>();
        serviceCollection.AddScoped<TweetDbService>();
        
        serviceCollection.AddSingleton<TweetHashtagService>();
    }
}