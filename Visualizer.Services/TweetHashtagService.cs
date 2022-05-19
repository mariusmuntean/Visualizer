using StackExchange.Redis;
using Tweetinvi.Events.V2;

namespace Visualizer.Services;

public class TweetHashtagService
{
    private const string HASHTAGS = "hashtags";
    private readonly IDatabase _database;

    public TweetHashtagService(IDatabase database)
    {
        _database = database;
    }

    public async Task AddHashtags(TweetV2ReceivedEventArgs tweetV2ReceivedEventArgs)
    {
        if (!tweetV2ReceivedEventArgs.Tweet?.Entities?.Hashtags?.Any() ?? true)
        {
            return;
        }

        var hashtags = tweetV2ReceivedEventArgs.Tweet.Entities.Hashtags.Select(h => h.Tag);
        foreach (var hashtag in hashtags)
        {
            await AddHashtag(hashtag);
        }
    }

    public async Task AddHashtag(string hashtag)
    {
        try
        {
            await _database.SortedSetIncrementAsync(new RedisKey(HASHTAGS), new RedisValue(hashtag), 1);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Failed to add/increment {hashtag} in the sorted set {HASHTAGS}. {ex.StackTrace}");
        }
    }

    public async Task<SortedSetEntry[]> GetTopHashtags(int amount = 10)
    {
        var range = await _database.SortedSetRangeByRankWithScoresAsync(new RedisKey(HASHTAGS), 0, amount, Order.Descending);
        return range;
    }
}