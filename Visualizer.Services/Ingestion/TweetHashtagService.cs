using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using StackExchange.Redis;
using Tweetinvi.Events.V2;

namespace Visualizer.Services.Ingestion;

public class TweetHashtagService
{
    private const string HASHTAGS = "hashtags";

    private readonly IDatabase _database;
    private readonly ISubject<ScoredHashtag> _hashtagAddedStream = new ReplaySubject<ScoredHashtag>(1);
    private readonly ConcurrentDictionary<int, ISubject<ScoredHashtag[]>> _amountToRankedHashtagsMap;

    public TweetHashtagService(IDatabase database)
    {
        _database = database;
        // AllHashtags = new ConcurrentStack<ScoredHashtag>();

        _amountToRankedHashtagsMap = new ConcurrentDictionary<int, ISubject<ScoredHashtag[]>>();
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
            var newScore = await _database.SortedSetIncrementAsync(new RedisKey(HASHTAGS), new RedisValue(hashtag), 1);

            // Single hashtag added
            var sortedSetEntry = new ScoredHashtag {Name = hashtag, Score = newScore};
            // AllHashtags.Push(sortedSetEntry);
            _hashtagAddedStream.OnNext(sortedSetEntry);

            // Ranked hashtags subscriptions - for each subscription: get the ranked hashtags (amount from subscription) and compare to previous value. If no previous value exists or if it is distinct from the current value, then provide the observer with new data.
            foreach (var (amount, rankedHashtagsSubject) in _amountToRankedHashtagsMap)
            {
                var currentRankedHashtags = await GetTopHashtags(amount);

                var previousKey = new RedisKey($"previous_ranked_hashtags_amount_{amount}");
                var previousExists = await _database.KeyExistsAsync(previousKey);
                if (!previousExists)
                {
                    // cache the current ranked hashtags for the current amount
                    await _database.SortedSetAddAsync(previousKey,
                        currentRankedHashtags.Select(scoredHashtag => new SortedSetEntry(new RedisValue(scoredHashtag.Name), scoredHashtag.Score)).ToArray());
                    rankedHashtagsSubject.OnNext(currentRankedHashtags);
                }
                else
                {
                    var previousValue = await _database.SortedSetRangeByRankWithScoresAsync(previousKey, 0, amount, Order.Descending);
                    if (previousValue?.Length != currentRankedHashtags.Length)
                    {
                        // delete the previous ranked hashtags for the current amount and cache the current ranked hashtags for the current amount
                        await _database.KeyDeleteAsync(previousKey);
                        await _database.SortedSetAddAsync(previousKey,
                            currentRankedHashtags.Select(scoredHashtag => new SortedSetEntry(new RedisValue(scoredHashtag.Name), scoredHashtag.Score)).ToArray());
                        rankedHashtagsSubject.OnNext(currentRankedHashtags);
                    }
                    else
                    {
                        // compare the previous ranked hashtags with the current value
                        for (var i = 0; i < currentRankedHashtags.Length; i++)
                        {
                            var currentRankedHashtag = currentRankedHashtags[i];
                            var previousRankedHashtag = previousValue[i];
                            if (currentRankedHashtag.Name != previousRankedHashtag.Element.ToString() ||
                                Math.Abs(currentRankedHashtag.Score - previousRankedHashtag.Score) > double.Epsilon)
                            {
                                // delete the previous ranked hashtags for the current amount and cache the current ranked hashtags for the current amount 
                                await _database.KeyDeleteAsync(previousKey);
                                await _database.SortedSetAddAsync(previousKey,
                                    currentRankedHashtags.Select(scoredHashtag => new SortedSetEntry(new RedisValue(scoredHashtag.Name), scoredHashtag.Score)).ToArray());
                                rankedHashtagsSubject.OnNext(currentRankedHashtags);
                                
                                break;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Failed to add/increment hashtag {hashtag} in the sorted set {HASHTAGS}. {ex.Message} {ex.StackTrace}");
        }
    }

    public IObservable<ScoredHashtag> GetHashtagAddedObservable()
    {
        return _hashtagAddedStream.AsObservable();
    }

    public IObservable<ScoredHashtag[]> GetRankedHashtagsObservable(int amount = 10)
    {
        return _amountToRankedHashtagsMap.GetOrAdd(amount, a => new ReplaySubject<ScoredHashtag[]>(1));
    }

    // public ConcurrentStack<ScoredHashtag> AllHashtags { get; }

    public async Task<ScoredHashtag[]> GetTopHashtags(int amount = 10)
    {
        var range = await _database.SortedSetRangeByRankWithScoresAsync(new RedisKey(HASHTAGS), 0, amount, Order.Descending);
        return range.Select(entry => new ScoredHashtag {Name = entry.Element.ToString(), Score = entry.Score}).ToArray();
    }

    public class ScoredHashtag
    {
        public string Name { get; set; }
        public double Score { get; set; }
    }
}