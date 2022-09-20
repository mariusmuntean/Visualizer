using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace Visualizer.API.Services.HostedServices;

/// <summary>
/// Apply runtime configuration to the Redis server by accessing the <see cref="IServer"/> instance from DI.
/// </summary>
public class RedisServerConfigurator : IHostedService
{
    private readonly IServer _redisServer;

    public RedisServerConfigurator(IServer redisServer)
    {
        _redisServer = redisServer;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Source https://redis.io/docs/manual/keyspace-notifications/
        await _redisServer.ConfigSetAsync("notify-keyspace-events", "KEA").ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
