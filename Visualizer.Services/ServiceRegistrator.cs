using Microsoft.Extensions.DependencyInjection;

namespace Visualizer.Services;

public static class ServiceRegistrator
{
    public static void Register(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<TweetBatchDownloadService>();
        serviceCollection.AddScoped<TweetGraphService>();
    }
}