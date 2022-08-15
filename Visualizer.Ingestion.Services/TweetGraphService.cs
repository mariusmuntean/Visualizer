using NRedisGraph;
using Tweetinvi.Events.V2;
using Visualizer.Shared.Models;

namespace Visualizer.Ingestion.Services;

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
}
