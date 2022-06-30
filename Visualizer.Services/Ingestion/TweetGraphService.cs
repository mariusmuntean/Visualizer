using NRedisGraph;
using Tweetinvi.Core.Extensions;
using Tweetinvi.Events.V2;
using Visualizer.Services.Extensions;

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

        var graphResult = new GraphResult {Nodes = new Dictionary<string, UserNode>(), Edges = new HashSet<MentionRelationship>()};
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

    public async Task<GraphResult> GetMentions(MentionFilterDto mentionFilterDto)
    {
        var (authorUserName, mentionedUserNames, amount) = mentionFilterDto;
        var queryUsers = (string.IsNullOrWhiteSpace(authorUserName), mentionedUserNames.IsNullOrEmpty()) switch
        {
            (true, true) => $"match (a:user)-[r:mentioned]->(b:user) return a,b,r LIMIT {amount}",
            (false, true) => $"match (a:user {{ {nameof(UserNode.UserName)} : '{authorUserName}' }})-[r:mentioned]->(b:user) return a,b,r LIMIT {amount} ",
            (true, false) => $"match (a:user)-[r:mentioned]->(b:user {{ {nameof(UserNode.UserName)} : '{mentionedUserNames.First()}' }}) return a,b,r LIMIT {amount} ",
            (false, false) =>
                $"match (a:user {{ {nameof(UserNode.UserName)} : '{authorUserName}' }})-[r:mentioned]->(b:user {{ {nameof(UserNode.UserName)} : '{mentionedUserNames.First()}' }}) return a,b,r LIMIT {amount} ",
        };

        Console.WriteLine(queryUsers);
        var queryUsersResult = await _redisGraph.QueryAsync("users", queryUsers);
        var records = queryUsersResult.ToList();

        var graphResult = new GraphResult {Nodes = new Dictionary<string, UserNode>(), Edges = new HashSet<MentionRelationship>()};
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

    public class GraphResult
    {
        public Dictionary<string, UserNode> Nodes { get; set; }

        public HashSet<MentionRelationship> Edges { get; set; }
    }

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

        public void Deconstruct(out string? authorUserName, out string[]? mentionedUserNames, out int amount)
        {
            authorUserName = this.AuthorUserName;
            mentionedUserNames = this.MentionedUserNames;
            amount = this.Amount;
        }
    }
}