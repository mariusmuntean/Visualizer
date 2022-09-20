﻿using Visualizer.Ingestion.Config;
using Visualizer.Ingestion.HostedServices;

namespace Visualizer.Ingestion;

public static class IngestionRegistrator
{
    public static void RegisterServices(WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.AddVisualizerSerilog();
        webApplicationBuilder.AddRedisGraph();
        webApplicationBuilder.AddTwitterClient();
        webApplicationBuilder.AddRedisOMConnectionProvider();
        webApplicationBuilder.AddRedlock();

        // Hosted Services
        webApplicationBuilder.Services.AddHostedService<GraphInitializer>();
        webApplicationBuilder.Services.AddHostedService<RedisOMIndexInitializer>();

        // Other

    }
}
