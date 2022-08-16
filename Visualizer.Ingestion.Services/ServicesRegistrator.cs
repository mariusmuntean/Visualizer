using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Visualizer.Ingestion.Services.Extensions;
using Visualizer.Ingestion.Services.Services;
using Visualizer.Ingestion.Services.Services.Impl;

namespace Visualizer.Ingestion.Services;

public static class ServicesRegistrator
{
    public static void Register(WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.AddMessagePublisher();

        // Ingestion
        webApplicationBuilder.Services.AddSingleton<ITwitterStreamService, TwitterStreamService>();
        webApplicationBuilder.Services.AddSingleton<ITweetGraphService, TweetGraphService>();
        webApplicationBuilder.Services.AddSingleton<ITweetDbService, TweetDbService>();
        webApplicationBuilder.Services.AddSingleton<ITweetHashtagService, TweetHashtagService>();
    }
}
