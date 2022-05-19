using System.Text.RegularExpressions;
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
            await Console.Error.WriteLineAsync($"Failed to add nodes: {e.StackTrace}");
        }
    }
}