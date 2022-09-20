using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Visualizer.API.Clients;
using Visualizer.API.Services.Services;
using Visualizer.API.Services.Services.Impl;

namespace Visualizer.API.Services.Config;

public static class IngestionServiceProxyConfig
{
    public static void AddIngestionServiceProxy(this WebApplicationBuilder webApplicationBuilder)
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

        webApplicationBuilder.Services.AddSingleton<IIngestionServiceProxy>(provider =>
        {
            var ingestionClient = provider.GetRequiredService<IIngestionClient>();
            var logger = provider.GetRequiredService<ILogger<IngestionServiceProxy>>();
            return new IngestionServiceProxy(ingestionClient, iSubscriber, logger);
        });
        webApplicationBuilder.Services.AddHostedService<IIngestionServiceProxy>(provider => provider.GetRequiredService<IIngestionServiceProxy>());
    }
}
