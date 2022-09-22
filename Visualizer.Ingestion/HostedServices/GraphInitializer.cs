using NRedisGraph;
using Visualizer.Shared.Models;

namespace Visualizer.Ingestion.HostedServices;

/// <summary>
/// Initializes the graph database, e.g. by creating indices.
/// </summary>
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
        result = await _redisGraph.QueryAsync("users", $"CREATE INDEX ON :user({nameof(UserNode.UserId)})");
        Console.WriteLine($"Created {result.Statistics.IndicesCreated} indices");
        result = await _redisGraph.QueryAsync("users", $"CREATE INDEX ON :user({nameof(UserNode.UserName)})");
        Console.WriteLine($"Created {result.Statistics.IndicesCreated} indices");

        result = await _redisGraph.QueryAsync("users", $"CREATE INDEX ON :mentioned({nameof(MentionRelationship.TweetId)})");
        Console.WriteLine($"Created {result.Statistics.IndicesCreated} indices");
        result = await _redisGraph.QueryAsync("users", $"CREATE INDEX ON :mentioned({nameof(MentionRelationship.RelationshipType)})");
        Console.WriteLine($"Created {result.Statistics.IndicesCreated} indices");

        result = await _redisGraph.QueryAsync("users", $"CREATE INDEX ON :was_mentioned_by({nameof(MentionRelationship.TweetId)})");
        Console.WriteLine($"Created {result.Statistics.IndicesCreated} indices");
        result = await _redisGraph.QueryAsync("users", $"CREATE INDEX ON :was_mentioned_by({nameof(MentionRelationship.RelationshipType)})");
        Console.WriteLine($"Created {result.Statistics.IndicesCreated} indices");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
