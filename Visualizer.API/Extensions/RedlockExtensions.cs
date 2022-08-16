using System.Net;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace Visualizer.API.Extensions;

public static class RedlockExtensions
{
    public static void AddRedlock(this WebApplicationBuilder webApplicationBuilder)
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
        var multiplexers = new List<RedLockMultiplexer> {muxer};
        var redlockFactory = RedLockFactory.Create(multiplexers);

        webApplicationBuilder.Services.AddSingleton<IDistributedLockFactory>(redlockFactory);
    }
}
