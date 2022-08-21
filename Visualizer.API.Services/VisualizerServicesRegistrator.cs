using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Visualizer.API.Services.Extensions;
using Visualizer.API.Services.Services.Impl;

namespace Visualizer.API.Services;

public static class VisualizerServicesRegistrator
{
    public static void Register(WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.AddIngestionServiceProxy();

        webApplicationBuilder.AddRedisOMConnectionProvider();

        // Query
        webApplicationBuilder.Services.AddScoped<TweetDbQueryService>();
    }
}
