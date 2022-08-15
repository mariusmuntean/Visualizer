// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Visualizer.Ingestion.Config;
using Visualizer.Ingestion.HostedServices;
using Visualizer.Ingestion.Services;

namespace Visualizer.Ingestion;

public static class Registrator
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
        webApplicationBuilder.Services.AddHostedService<IndexInitializer>();

        // Ingestion
        webApplicationBuilder.Services.AddSingleton<TwitterStreamService>();
        webApplicationBuilder.Services.AddSingleton<TweetGraphService>();
        webApplicationBuilder.Services.AddSingleton<TweetDbService>();
        webApplicationBuilder.Services.AddSingleton<TweetHashtagService>();
    }
}
