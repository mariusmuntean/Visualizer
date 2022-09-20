using Redis.OM;
using Visualizer.Shared.Models;

namespace Visualizer.Ingestion.HostedServices;

public class RedisOMIndexInitializer : IHostedService
{
    private readonly RedisConnectionProvider _redisConnectionProvider;

    public RedisOMIndexInitializer(RedisConnectionProvider redisConnectionProvider)
    {
        _redisConnectionProvider = redisConnectionProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _redisConnectionProvider.Connection.DropIndexAsync(typeof(TweetModel)).ConfigureAwait(false);
        await _redisConnectionProvider.Connection.CreateIndexAsync(typeof(TweetModel)).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
