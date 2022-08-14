using NRedisGraph;
using Tweetinvi.Core.Extensions;
using Tweetinvi.Events.V2;
using Visualizer.Services.Extensions;
using Path = NRedisGraph.Path;

namespace Visualizer.Services.Ingestion;

public class TweetGraphService
{
    private readonly RedisGraph _redisGraph;

    public TweetGraphService(RedisGraph redisGraph)
    {
        _redisGraph = redisGraph;
    }

    public async Task AddNodes(TweetV2ReceivedEventArgs tweetV2ReceivedEventArgs)
    {
        var tweet = tweetV2ReceivedEventArgs.Tweet;
        var includes = tweetV2ReceivedEventArgs.Includes;

        var user = includes.Users.FirstOrDefault(u => u.Id == tweet.AuthorId);
        if (user is null)
        {
            return;
        }

        try
        {
            var transaction = _redisGraph.Multi();

            // Add a node for the tweet author
            var userName = Uri.EscapeDataString(user.Username);
            var addUserQuery = $"CREATE(:user{{{nameof(UserNode.UserId)}:'{user.Id}', {nameof(UserNode.UserName)}:'{userName}'}})";
            // Console.WriteLine(addUserQuery);
            await transaction.QueryAsync("users", addUserQuery);

            var otherUsers = includes.Users.Where(u => u.Id != tweet.AuthorId);
            if (otherUsers.Any() == false)
            {
                return;
            }

            // Add a node and a relationship for each referenced user
            foreach (var otherUser in otherUsers)
            {
                var otherUserName = Uri.EscapeDataString(otherUser.Username);

                var addOtherUserQuery = $"CREATE(:user{{{nameof(UserNode.UserId)}:'{otherUser.Id}', {nameof(UserNode.UserName)}:'{otherUserName}'}})";
                // Console.WriteLine(addOtherUserQuery);
                await transaction.QueryAsync("users", addOtherUserQuery);

                var addOtherUserRelQuery =
                    $"MATCH (a:user {{ {nameof(UserNode.UserId)} : '{user.Id}' }}), (b:user {{ {nameof(UserNode.UserId)} : '{otherUser.Id}' }}) CREATE (a)-[:mentioned {{ {nameof(MentionRelationship.TweetId)} : {tweet.Id} }} ]->(b)";
                // Console.WriteLine(addOtherUserRelQuery);
                await transaction.QueryAsync("users", addOtherUserRelQuery);

                var addOtherUserInverseRelQuery =
                    $"MATCH (a:user {{ {nameof(UserNode.UserId)} : '{otherUser.Id}' }}), (b:user {{ {nameof(UserNode.UserId)} : '{user.Id}' }}) CREATE (a)-[:was_mentioned_by {{ {nameof(MentionRelationship.TweetId)} : {tweet.Id} }} ]->(b)";
                // Console.WriteLine(addOtherUserInverseRelQuery);
                await transaction.QueryAsync("users", addOtherUserInverseRelQuery);
            }

            var results = await transaction.ExecAsync();
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync($"Failed to add nodes: {e.Message} {e.Source} {e.StackTrace} {e.Data}");
        }
    }

    public async Task<GraphResult> GetNodes(int amount = 20)
    {
        var queryUsers = $"match (a:user)-[r:mentioned|:was_mentioned_by]->(b:user) return a,b,r LIMIT {amount}";
        Console.WriteLine(queryUsers);
        var queryUsersResult = await _redisGraph.QueryAsync("users", queryUsers);
        var records = queryUsersResult.ToList();

        var graphResult = new GraphResult { Nodes = new Dictionary<string, UserNode>(), Edges = new HashSet<MentionRelationship>() };
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

    public async Task<GraphResult> GetMentions(MentionFilterDto mentionFilterDto)
    {
        var (authorUserName, mentionedUserNames, amount, minHops, maxHops) = mentionFilterDto;
        if (minHops > maxHops)
        {
            return new GraphResult();
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

        var graphResult = new GraphResult { Statistics = new GraphResultStatistics(QueryInternalExecutionTime: queryUsersResult.Statistics.QueryInternalExecutionTime) };
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

    public class GraphResult
    {
        public Dictionary<string, UserNode> Nodes { get; set; } = new Dictionary<string, UserNode>();

        public HashSet<MentionRelationship> Edges { get; set; } = new HashSet<MentionRelationship>();

        public GraphResultStatistics? Statistics { get; set; }
    }

    public record GraphResultStatistics(string QueryInternalExecutionTime);

    public record UserNode(string UserId, string UserName);

    public enum MentionRelationshipType
    {
        Mentioned,
        Was_Mentioned_By
    }

    public record MentionRelationship(string FromUserId, string ToUserId, string TweetId, MentionRelationshipType RelationshipType);

    public class MentionFilterDto
    {
        public string? AuthorUserName { get; set; }
        public string[]? MentionedUserNames { get; set; }
        public int Amount { get; set; }

        public int MinHops { get; set; } = 1;
        public int MaxHops { get; set; } = 10;

        public void Deconstruct(out string? authorUserName, out string[]? mentionedUserNames, out int amount, out int minHops, out int maxHops)
        {
            authorUserName = AuthorUserName;
            mentionedUserNames = MentionedUserNames;
            amount = Amount;
            minHops = MinHops;
            maxHops = MaxHops;
        }
    }
}