using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Tweetinvi.Events.V2;
using Visualizer.Shared.Constants;

namespace Visualizer.Ingestion.Services.Services.Impl;

internal class TweetHashtagService : ITweetHashtagService
{
    private readonly IDatabase _database;
    private readonly ILogger<TweetHashtagService> _logger;

    public TweetHashtagService(IDatabase database, ILogger<TweetHashtagService> logger)
    {
        _database = database;
        _logger = logger;
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
            var _ = await _database.SortedSetIncrementAsync(new RedisKey(HashtagConstants.RankedHashtagsKey), new RedisValue(hashtag), 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add/increment hashtag {Hashtag} in the sorted set {RankedHashtagsKey}. {ExMessage}", hashtag, HashtagConstants.RankedHashtagsKey, ex.Message);
        }
    }
}
