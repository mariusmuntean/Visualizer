using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Visualizer.API.Services.HostedServices;

namespace Visualizer.API.Services.Config;

public static class RedisServerConfig
{
    /// <summary>
    /// Adds an <see cref="IServer"/> instance to the DI container and registers <see cref="RedisServerConfigurator"/> as a hosted service.
    /// </summary>
    /// <param name="webApplicationBuilder"></param>
    public static void AddRedisServerAndConfigurator(this WebApplicationBuilder webApplicationBuilder)
    {
        var host = webApplicationBuilder.Configuration.GetSection("Redis")["Host"];
        var port = webApplicationBuilder.Configuration.GetSection("Redis")["Port"];
        var configurationOptions = new ConfigurationOptions
        {
            EndPoints = new EndPointCollection {new DnsEndPoint(host, int.Parse(port))},
            SyncTimeout = 10000,
            AsyncTimeout = 10000,
            IncludePerformanceCountersInExceptions = true,
            IncludeDetailInExceptions = true,
            AllowAdmin = true
        };
        var muxer = ConnectionMultiplexer.Connect(configurationOptions);
        var iServer = muxer.GetServer(host, int.Parse(port));

        // register the iServer instance
        webApplicationBuilder.Services.AddSingleton(iServer);

        // register the server configurator as a hosted service.
        webApplicationBuilder.Services.AddHostedService<RedisServerConfigurator>();
    }
}
