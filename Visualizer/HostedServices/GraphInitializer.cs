using NRedisGraph;

namespace Visualizer.HostedServices;

public class GraphInitializer : IHostedService
{
    private readonly RedisGraph _redisGraph;

    public GraphInitializer(RedisGraph redisGraph)
    {
        _redisGraph = redisGraph;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var result = await _redisGraph.QueryAsync("users", "CREATE INDEX ON :user(id)");
        Console.WriteLine($"Created {result.Statistics.IndicesCreated} indices");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}