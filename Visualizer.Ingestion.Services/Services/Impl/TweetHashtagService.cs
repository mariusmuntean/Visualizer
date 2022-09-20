using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Tweetinvi.Events.V2;
using Visualizer.Shared.Constants;

namespace Visualizer.Ingestion.Services.Services.Impl;

internal class TweetHashtagService : ITweetHashtagService
{
    private readonly IDatabase _database;
    private readonly IHashtagRankedMessagePublisher _hashtagRankedMessagePublisher;
    private readonly ILogger<TweetHashtagService> _logger;

    public TweetHashtagService(IDatabase database, IHashtagRankedMessagePublisher hashtagRankedMessagePublisher, ILogger<TweetHashtagService> logger)
    {
        _database = database;
        _hashtagRankedMessagePublisher = hashtagRankedMessagePublisher;
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
            // Add or increment the hashtag in the sorted set
            var sortedSetKey = new RedisKey(HashtagConstants.RankedHashtagsSortedSetKey);
            var sortedSetValue = new RedisValue(hashtag);
            var newRank = await _database.SortedSetIncrementAsync(sortedSetKey, sortedSetValue, 1).ConfigureAwait(false);

            // Publish a message with the ranked hashtag
            await _hashtagRankedMessagePublisher.PublishRankedHashtagMessage(hashtag, (int) newRank).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add/increment hashtag {Hashtag} in the sorted set {RankedHashtagsKey}. {ExMessage}", hashtag, HashtagConstants.RankedHashtagsSortedSetKey, ex.Message);
        }
    }
}
