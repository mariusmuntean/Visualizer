using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Visualizer.API.Services.Extensions;
using Visualizer.API.Services.Services;
using Visualizer.API.Services.Services.Impl;

namespace Visualizer.API.Services;

public static class VisualizerServicesRegistrator
{
    public static void Register(WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.AddIngestionServiceProxy();

        webApplicationBuilder.AddRedisOMConnectionProvider();
        webApplicationBuilder.AddRedisGraph();
        webApplicationBuilder.AddRedisDatabase();

        // Query
        webApplicationBuilder.Services.AddScoped<ITweetDbQueryService, TweetDbQueryService>();
        webApplicationBuilder.Services.AddScoped<ITweetHashtagService, TweetHashtagService>();
        webApplicationBuilder.Services.AddScoped<ITweetGraphService, TweetGraphService>();
    }
}
