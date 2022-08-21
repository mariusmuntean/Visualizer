using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using StackExchange.Redis;
using Visualizer.API.Services.DTOs;

namespace Visualizer.API.Services.Services.Impl;

internal class TweetHashtagService : ITweetHashtagService
{
    private const string HASHTAGS = "hashtags";

    private readonly IDatabase _database;
    private readonly ISubject<ScoredHashtag> _hashtagAddedStream = new ReplaySubject<ScoredHashtag>(1);
    private readonly ConcurrentDictionary<int, ReplaySubject<ScoredHashtag[]>> _amountToRankedHashtagsMap;

    public TweetHashtagService(IDatabase database)
    {
        _database = database;
        // AllHashtags = new ConcurrentStack<ScoredHashtag>();

        _amountToRankedHashtagsMap = new ConcurrentDictionary<int, ReplaySubject<ScoredHashtag[]>>();
    }

    public IObservable<ScoredHashtag> GetHashtagAddedObservable()
    {
        return _hashtagAddedStream.AsObservable().Sample(TimeSpan.FromSeconds(5));
    }

    public IObservable<ScoredHashtag[]> GetRankedHashtagsObservable(int amount = 10)
    {
        var rankedHashtagsObservable = _amountToRankedHashtagsMap.GetOrAdd(amount, a => new ReplaySubject<ScoredHashtag[]>(1));
        return rankedHashtagsObservable.AsObservable().Sample(TimeSpan.FromSeconds(5));
    }


    private async Task ReplaceCurrentRankedHashtags(RedisKey previousKey, ScoredHashtag[] currentRankedHashtags)
    {
        await _database.KeyDeleteAsync(previousKey);
        await CacheCurrentRankedHashtags(previousKey, currentRankedHashtags);
    }

    private async Task CacheCurrentRankedHashtags(RedisKey previousKey, ScoredHashtag[] currentRankedHashtags)
    {
        var sortedSetEntries = currentRankedHashtags.Select(scoredHashtag => new SortedSetEntry(new RedisValue(scoredHashtag.Name), scoredHashtag.Score)).ToArray();
        await _database.SortedSetAddAsync(previousKey, sortedSetEntries);
    }

    private async Task<bool> TrimSubscriptionIfNecessary(ReplaySubject<ScoredHashtag[]> rankedHashtagsSubject, int amount, RedisKey previousKey)
    {
        if (rankedHashtagsSubject.HasObservers == false)
        {
            if (_amountToRankedHashtagsMap.TryRemove(amount, out var _))
            {
                Console.WriteLine($"Removed subscription {amount}");
                if (await _database.KeyDeleteAsync(previousKey))
                {
                    Console.WriteLine($"Removed the sorted set for {amount}");
                }

                return true;
            }
        }

        return false;
    }

    public async Task<ScoredHashtag[]> GetTopHashtags(int amount = 10)
    {
        var range = await _database.SortedSetRangeByRankWithScoresAsync(new RedisKey(HASHTAGS), 0, amount, Order.Descending);
        return range.Select(entry => new ScoredHashtag {Name = entry.Element.ToString(), Score = entry.Score}).ToArray();
    }
}
