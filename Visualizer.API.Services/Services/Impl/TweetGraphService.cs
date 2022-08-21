using NRedisGraph;
using Tweetinvi.Core.Extensions;
using Visualizer.API.Services.DTOs;
using Visualizer.API.Services.Extensions;
using Visualizer.Shared.Models;
using Path = NRedisGraph.Path;

namespace Visualizer.API.Services.Services.Impl;

internal class TweetGraphService : ITweetGraphService
{
    private readonly RedisGraph _redisGraph;

    public TweetGraphService(RedisGraph redisGraph)
    {
        _redisGraph = redisGraph;
    }

    public async Task<GraphResultDto> GetNodes(int amount = 20)
    {
        var queryUsers = $"match (a:user)-[r:mentioned|:was_mentioned_by]->(b:user) return a,b,r LIMIT {amount}";
        Console.WriteLine(queryUsers);
        var queryUsersResult = await _redisGraph.QueryAsync("users", queryUsers);
        var records = queryUsersResult.ToList();

        var graphResult = new GraphResultDto { Nodes = new Dictionary<string, UserNode>(), Edges = new HashSet<MentionRelationship>() };
        foreach (var record in records)
        {
            var nodes = record.Values.OfType<Node>().ToArray();
            if (!nodes.Any())
            {
                continue;
            }

            var firstNode = nodes.First();
            var firstUserNode = firstNode.ToUserNode();
            var idA = firstUserNode.UserId;

            if (!graphResult.Nodes.ContainsKey(idA))
            {
                graphResult.Nodes.Add(idA, firstUserNode);
            }

            var secondNode = nodes.Skip(1).First();
            var secondUserNode = secondNode.ToUserNode();
            var idB = secondUserNode.UserId;

            if (!graphResult.Nodes.ContainsKey(idB))
            {
                graphResult.Nodes.Add(idB, secondUserNode);
            }

            var relationship = record.Values.OfType<Edge>().ToArray().Single();
            graphResult.Edges.Add(relationship.ToMentionRelationship(idA, idB));
        }

        return graphResult;
    }

    public async Task<long> CountUsers()
    {
        var query = "match (u:user) return count(u)";
        var countUsersQueryResult = await _redisGraph.QueryAsync("users", query);
        var records = countUsersQueryResult.ToList();

        object? maybeCount = records.FirstOrDefault()?.Values.FirstOrDefault();
        return maybeCount is null ? 0 : (long)maybeCount;
    }

    public async Task<GraphResultDto> GetMentions(MentionFilterDto mentionFilterDto)
    {
        var (authorUserName, mentionedUserNames, amount, minHops, maxHops) = mentionFilterDto;
        if (minHops > maxHops)
        {
            return new GraphResultDto();
        }
        var queryUsers = (string.IsNullOrWhiteSpace(authorUserName), mentionedUserNames.IsNullOrEmpty()) switch
        {
            (true, true) => $"match p=(a:user)-[r:mentioned*{minHops}..{maxHops}]->(b:user) return p LIMIT {amount}",
            (false, true) => $"match p=(a:user {{ {nameof(UserNode.UserName)} : '{authorUserName}' }})-[r:mentioned*{minHops}..{maxHops}]->(b:user) return p LIMIT {amount} ",
            (true, false) => $"match p=(a:user)-[r:mentioned*{minHops}..{maxHops}]->(b:user {{ {nameof(UserNode.UserName)} : '{mentionedUserNames.First()}' }}) return p LIMIT {amount} ",
            (false, false) =>
                $"match p = (a:user {{ {nameof(UserNode.UserName)} : '{authorUserName}' }})-[r:mentioned*{minHops}..{maxHops}]->(b:user {{ {nameof(UserNode.UserName)} : '{mentionedUserNames.First()}' }}) return p LIMIT {amount} ",
        };

        Console.WriteLine(queryUsers);
        var queryUsersResult = await _redisGraph.QueryAsync("users", queryUsers);
        var records = queryUsersResult.ToList();

        var graphResult = new GraphResultDto { Statistics = new GraphResultStatisticsDto(QueryInternalExecutionTime: queryUsersResult.Statistics.QueryInternalExecutionTime) };
        foreach (var record in records)
        {
            var paths = record.Values.OfType<Path>().ToArray();
            foreach (var path in paths)
            {
                var nodesArr = path.Nodes.ToArray();
                var edgesArr = path.Edges.ToArray();
                Node prevNode = null;
                for (var i = 0; i < nodesArr.Length; i++)
                {
                    var currentNode = nodesArr[i];
                    var currentUserNode = currentNode.ToUserNode();
                    var currentUserId = currentUserNode.UserId;
                    if (!graphResult.Nodes.ContainsKey(currentUserId))
                    {
                        graphResult.Nodes.Add(currentUserId, currentUserNode);
                    }

                    if (prevNode is not null)
                    {
                        var prevUserNode = prevNode.ToUserNode();
                        var prevUserId = prevUserNode.UserId;
                        var edge = Array.Find(edgesArr, e => e.Source == prevNode.Id && e.Destination == currentNode.Id);
                        if (edge is not null)
                        {
                            graphResult.Edges.Add(edge.ToMentionRelationship(prevUserId, currentUserId));
                        }
                    }

                    prevNode = currentNode;
                }
            }


            // var nodes = record.Values.OfType<Node>().ToArray();
            // if (!nodes.Any())
            // {
            //     continue;
            // }
            //
            // var firstNode = nodes.First();
            // var firstUserNode = firstNode.ToUserNode();
            // var idA = firstUserNode.UserId;
            //
            // if (!graphResult.Nodes.ContainsKey(idA))
            // {
            //     graphResult.Nodes.Add(idA, firstUserNode);
            // }
            //
            // var secondNode = nodes.Skip(1).First();
            // var secondUserNode = secondNode.ToUserNode();
            // var idB = secondUserNode.UserId;
            //
            // if (!graphResult.Nodes.ContainsKey(idB))
            // {
            //     graphResult.Nodes.Add(idB, secondUserNode);
            // }
            //
            // var relationship = record.Values.OfType<Edge>().ToArray().Single();
            // graphResult.Edges.Add(relationship.ToMentionRelationship(idA, idB));
        }

        return graphResult;
    }
}
