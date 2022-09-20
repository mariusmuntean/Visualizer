using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Visualizer.Ingestion.Services.Services;
using Visualizer.Ingestion.Services.Services.Impl;

namespace Visualizer.Ingestion.Services.Extensions;

public static class PubSubConfig
{
    public static void AddPubSub(this WebApplicationBuilder webApplicationBuilder)
    {
        var host = webApplicationBuilder.Configuration.GetSection("Redis")["Host"];
        var port = webApplicationBuilder.Configuration.GetSection("Redis")["Port"];
        var configurationOptions = new ConfigurationOptions
        {
            EndPoints = new EndPointCollection {new DnsEndPoint(host, int.Parse(port))},
            SyncTimeout = 10000,
            AsyncTimeout = 10000,
            IncludePerformanceCountersInExceptions = true,
            IncludeDetailInExceptions = true
        };
        var muxer = ConnectionMultiplexer.Connect(configurationOptions);

        var iSubscriber = muxer.GetSubscriber();
        webApplicationBuilder.Services.AddSingleton<ISubscriber>(iSubscriber);
    }
}
