using NRedisGraph;
using Visualizer.Services.Ingestion;

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
        result = await _redisGraph.QueryAsync("users", $"CREATE INDEX ON :user({nameof(TweetGraphService.UserNode.UserId)})");
        Console.WriteLine($"Created {result.Statistics.IndicesCreated} indices");
        result = await _redisGraph.QueryAsync("users", $"CREATE INDEX ON :user({nameof(TweetGraphService.UserNode.UserName)})");
        Console.WriteLine($"Created {result.Statistics.IndicesCreated} indices");

        result = await _redisGraph.QueryAsync("users", $"CREATE INDEX ON :mentioned({nameof(TweetGraphService.MentionRelationship.TweetId)})");
        Console.WriteLine($"Created {result.Statistics.IndicesCreated} indices");
        result = await _redisGraph.QueryAsync("users", $"CREATE INDEX ON :mentioned({nameof(TweetGraphService.MentionRelationship.RelationshipType)})");
        Console.WriteLine($"Created {result.Statistics.IndicesCreated} indices");

        result = await _redisGraph.QueryAsync("users", $"CREATE INDEX ON :was_mentioned_by({nameof(TweetGraphService.MentionRelationship.TweetId)})");
        Console.WriteLine($"Created {result.Statistics.IndicesCreated} indices");
        result = await _redisGraph.QueryAsync("users", $"CREATE INDEX ON :was_mentioned_by({nameof(TweetGraphService.MentionRelationship.RelationshipType)})");
        Console.WriteLine($"Created {result.Statistics.IndicesCreated} indices");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}