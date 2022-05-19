using Mapster;
using Redis.OM;
using Tweetinvi.Events.V2;
using Visualizer.Model;

namespace Visualizer.Services;

public class TweetDbService
{
    private readonly RedisConnectionProvider _redisConnectionProvider;

    public TweetDbService(RedisConnectionProvider redisConnectionProvider)
    {
        _redisConnectionProvider = redisConnectionProvider;
    }

    public async Task<string> Store(TweetV2ReceivedEventArgs tweetV2ReceivedEventArgs)
    {
        string internalId;
        try
        {
            var tweetCollection = _redisConnectionProvider.RedisCollection<TweetModel>();
            var tweetModel = tweetV2ReceivedEventArgs.Tweet.Adapt<TweetModel>();
            internalId = await tweetCollection.InsertAsync(tweetModel);
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync($"Failed to process tweet event {tweetV2ReceivedEventArgs?.Tweet?.Id}");
            return null;
        }

        return internalId;
    }
}