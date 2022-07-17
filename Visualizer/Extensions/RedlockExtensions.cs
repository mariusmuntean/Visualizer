using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace Visualizer.Extensions;

public static class RedlockExtensions
{
    public static void AddRedlock(this WebApplicationBuilder webApplicationBuilder)
    {
        var configurationOptions = ConfigurationOptions.Parse("localhost");
        configurationOptions.SyncTimeout = 10000;
        configurationOptions.AsyncTimeout = 10000;
        configurationOptions.IncludePerformanceCountersInExceptions = true;
        configurationOptions.IncludeDetailInExceptions = true;

        var muxer = ConnectionMultiplexer.Connect(configurationOptions);
        var multiplexers = new List<RedLockMultiplexer>() {muxer};
        var redlockFactory = RedLockFactory.Create(multiplexers);

        webApplicationBuilder.Services.AddSingleton<IDistributedLockFactory>(redlockFactory);
    }
}
