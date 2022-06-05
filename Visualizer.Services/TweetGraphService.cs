using NRedisGraph;
using Tweetinvi.Events.V2;

namespace Visualizer.Services;

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
            // Add a node for the tweet author
            var userName = Uri.EscapeDataString(user.Name);
            var addUserQuery = $"CREATE(:user{{id:'{user.Id}', userName:'{userName}'}})";
            Console.WriteLine(addUserQuery);
            var result = await _redisGraph.QueryAsync("users", addUserQuery);

            var otherUsers = includes.Users.Where(u => u.Id != tweet.AuthorId);
            if (otherUsers.Any() == false)
            {
                return;
            }

            // Add a node and a relationship for each referenced user
            foreach (var otherUser in otherUsers)
            {
                var otherUserName = Uri.UnescapeDataString(otherUser.Name);

                var addOtherUserQuery = $"CREATE(:user{{id:'{otherUser.Id}', userName:'{otherUserName}'}})";
                Console.WriteLine(addOtherUserQuery);
                var addOtherUserResult = await _redisGraph.QueryAsync("users", addOtherUserQuery);

                var addOtherUserRelQuery = $"MATCH (a:user {{ id : '{user.Id}' }}), (b:user {{ id : '{otherUser.Id}' }}) CREATE (a)-[:mentioned]->(b)";
                Console.WriteLine(addOtherUserRelQuery);
                var addOtherUserRelationshipResult = await _redisGraph.QueryAsync("users", addOtherUserRelQuery);

                var addOtherUserInverseRelQuery = $"MATCH (a:user {{ id : '{otherUser.Id}' }}), (b:user {{ id : '{user.Id}' }}) CREATE (a)-[:was_mentioned_by]->(b)";
                Console.WriteLine(addOtherUserInverseRelQuery);
                var addOtherUserInverseRelationshipResult = await _redisGraph.QueryAsync("users", addOtherUserInverseRelQuery);
            }
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync($"Failed to add nodes: {e.Message} {e.Source} {e.StackTrace} {e.Data}");
        }
    }

    public async Task<GraphResult> GetNodes(int amount = 20)
    {
        var queryUsers = $"match (a:user)-[r:mentioned]->(b:user) return a,b LIMIT {amount}";
        Console.WriteLine(queryUsers);
        var queryUsersResult = await _redisGraph.QueryAsync("users", queryUsers);
        var records = queryUsersResult.ToList();

        var graphResult = new GraphResult { Edges = new HashSet<(string fromId, string toId)>(), Nodes = new Dictionary<string, object>() };
        foreach (var record in records)
        {
            var results = record.Values.Select(v => v as Node);

            var idA = results.First().PropertyMap["id"].Value.ToString();
            var usernameA = results.First().PropertyMap["userName"].Value.ToString();

            var idB = results.Skip(1).First().PropertyMap["id"].Value.ToString();
            var usernameB = results.Skip(1).First().PropertyMap["userName"].Value.ToString();


            if (!graphResult.Nodes.ContainsKey(idA))
            {
                graphResult.Nodes.Add(idA, new { username = usernameA });
            }

            if (!graphResult.Nodes.ContainsKey(idB))
            {
                graphResult.Nodes.Add(idB, new { username = usernameB });
            }

            graphResult.Edges.Add((idA, idB));
        }
        
        return graphResult;
    }

    public class GraphResult
    {
        public Dictionary<string, object> Nodes { get; set; }

        public HashSet<(string fromId, string toId)> Edges { get; set; }
    }
}