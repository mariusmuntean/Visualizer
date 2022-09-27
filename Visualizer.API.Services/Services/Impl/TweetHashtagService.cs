using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Newtonsoft.Json;
using StackExchange.Redis;
using Visualizer.Shared.Constants;
using Visualizer.Shared.Models;

namespace Visualizer.API.Services.Services.Impl;

internal class TweetHashtagService : ITweetHashtagService
{
    private readonly IDatabase _database;
    private readonly ISubject<RankedHashtag> _rankedHashtagStream;
    private readonly ConcurrentDictionary<int, ReplaySubject<RankedHashtag[]>> _amountToRankedHashtagsMap;

    public TweetHashtagService(IDatabase database, ISubscriber subscriber)
    {
        _database = database;

        _rankedHashtagStream = new ReplaySubject<RankedHashtag>(1);
        _amountToRankedHashtagsMap = new ConcurrentDictionary<int, ReplaySubject<RankedHashtag[]>>();

        var rankedHashtagChannel = new RedisChannel(HashtagConstants.RankedHashtagChannelName, RedisChannel.PatternMode.Literal);
        var rankedHashtagChannelMessageQueue = subscriber.Subscribe(rankedHashtagChannel);
        rankedHashtagChannelMessageQueue.OnMessage(OnRankedHashtagMessage);
    }

    public IObservable<RankedHashtag> GetRankedHashtagObservable(double samplingIntervalSeconds)
    {
        return _rankedHashtagStream.AsObservable().Sample(TimeSpan.FromSeconds(samplingIntervalSeconds));
    }

    public IObservable<RankedHashtag[]> GetTopRankedHashtagsObservable(int amount = 10)
    {
        var rankedHashtagsObservable = _amountToRankedHashtagsMap.GetOrAdd(amount, a => new ReplaySubject<RankedHashtag[]>(1));
        // return rankedHashtagsObservable.AsObservable().Sample(TimeSpan.FromSeconds(1));
        return rankedHashtagsObservable.AsObservable();
    }

    public async Task<RankedHashtag[]> GetTopHashtags(int amount = 10)
    {
        var sortedSetRange = await _database.SortedSetRangeByRankWithScoresAsync(new RedisKey(HashtagConstants.RankedHashtagsSortedSetKey), 0, amount - 1, Order.Descending).ConfigureAwait(false);
        return ToRankedHashtags(sortedSetRange);
    }

    private async void OnRankedHashtagMessage(ChannelMessage message)
    {
        // Every time a hashtag is ranked, inform any subscriber.
        string rankedHashtagStr = message.Message;
        var rankedHashtag = JsonConvert.DeserializeObject<RankedHashtag>(rankedHashtagStr);
        _rankedHashtagStream.OnNext(rankedHashtag);

        // Every time a hashtag is ranked, check if any subscribers need to be informed or if housekeeping can be performed.
        foreach (var (amount, rankedHashtagStream) in _amountToRankedHashtagsMap)
        {
            // Housekeeping if necessary - removes the subscription from the dictionary and deleted the temporary sorted set
            var previousRankedHashtagsForCurrentAmountKey = new RedisKey($"PreviousRankedHashtagsForAmount:{amount}");
            var housekeepingPerformed = await TrimSubscriptionIfNecessary(rankedHashtagStream, amount, previousRankedHashtagsForCurrentAmountKey).ConfigureAwait(false);
            if (housekeepingPerformed)
            {
                continue;
            }

            // Compare the current top "amount" of ranked hashtags with the previous top "amount".
            // Inform subscribers if distinct and then store new top "amount" of ranked hashtags.
            var currentTopRankedHashtags = await GetTopHashtags(amount).ConfigureAwait(false);
            var previousTopRankedHashtags = await GetRankedHashtags(previousRankedHashtagsForCurrentAmountKey).ConfigureAwait(false);
            var distinct = currentTopRankedHashtags.Length != previousTopRankedHashtags.Length || currentTopRankedHashtags.Zip(previousTopRankedHashtags)
                .Any(tuple => tuple.First.Name != tuple.Second.Name || Math.Abs(tuple.First.Rank - tuple.Second.Rank) > float.Epsilon);
            if (distinct)
            {
                // Inform subscribers
                rankedHashtagStream.OnNext(currentTopRankedHashtags);

                // Store current ranked hashtags as previous
                await ReplaceCurrentRankedHashtags(previousRankedHashtagsForCurrentAmountKey, currentTopRankedHashtags).ConfigureAwait(false);
            }
        }
    }

    private async Task<RankedHashtag[]> GetRankedHashtags(RedisKey key)
    {
        var sortedSet = await _database.SortedSetRangeByRankWithScoresAsync(key, 0, order: Order.Descending).ConfigureAwait(false);
        return ToRankedHashtags(sortedSet);
    }

    private async Task ReplaceCurrentRankedHashtags(RedisKey previousKey, RankedHashtag[] currentRankedHashtags)
    {
        await _database.KeyDeleteAsync(previousKey).ConfigureAwait(false);

        var sortedSetEntries = currentRankedHashtags.Select(scoredHashtag => new SortedSetEntry(new RedisValue(scoredHashtag.Name), scoredHashtag.Rank)).ToArray();
        await _database.SortedSetAddAsync(previousKey, sortedSetEntries).ConfigureAwait(false);
    }

    private async Task<bool> TrimSubscriptionIfNecessary(ReplaySubject<RankedHashtag[]> rankedHashtagsSubject, int amount, RedisKey previousKey)
    {
        if (rankedHashtagsSubject.HasObservers == false)
        {
            if (_amountToRankedHashtagsMap.TryRemove(amount, out var _))
            {
                Console.WriteLine($"Removed subscription {amount}");
                if (await _database.KeyDeleteAsync(previousKey).ConfigureAwait(false))
                {
                    Console.WriteLine($"Removed the sorted set for {amount}");
                }

                return true;
            }
        }

        return false;
    }

    private static RankedHashtag[] ToRankedHashtags(SortedSetEntry[] range)
    {
        return range.Select(entry => new RankedHashtag {Name = entry.Element.ToString(), Rank = entry.Score}).ToArray();
    }
}
