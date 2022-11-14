using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Visualizer.Ingestion.Services.Extensions;
using Visualizer.Ingestion.Services.Services;
using Visualizer.Ingestion.Services.Services.Impl;

namespace Visualizer.Ingestion.Services;

public static class IngestionServicesRegistrator
{
    public static void Register(WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.AddPubSub();

        webApplicationBuilder.Services.AddSingleton<IStreamingStatusMessagePublisher, StreamingStatusMessagePublisher>();
        webApplicationBuilder.Services.AddSingleton<IHashtagRankedMessagePublisher, HashtagRankedMessagePublisher>();

        // Ingestion
        //webApplicationBuilder.Services.AddSingleton<ITwitterStreamService, TwitterStreamService>();
        webApplicationBuilder.Services.AddSingleton<ITwitterStreamService, TwitterFilteredStreamService>();

        webApplicationBuilder.Services.AddSingleton<ITweetGraphService, TweetGraphService>();
        webApplicationBuilder.Services.AddSingleton<ITweetDbService, TweetDbService>();
        webApplicationBuilder.Services.AddSingleton<ITweetHashtagService, TweetHashtagService>();
    }
}
