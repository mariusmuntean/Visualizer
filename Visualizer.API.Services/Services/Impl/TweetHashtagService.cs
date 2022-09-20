using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Newtonsoft.Json;
using StackExchange.Redis;
using Visualizer.API.Services.DTOs;
using Visualizer.Shared.Constants;
using Visualizer.Shared.Models;

namespace Visualizer.API.Services.Services.Impl;

internal class TweetHashtagService : ITweetHashtagService
{
    private readonly IDatabase _database;
    private readonly ISubscriber _subscriber;
    private readonly ISubject<RankedHashtag> _rankedHashtagStream = new ReplaySubject<RankedHashtag>(1);
    private readonly ConcurrentDictionary<int, ReplaySubject<RankedHashtag[]>> _amountToRankedHashtagsMap;

    private RedisChannel _rankedHashtagChannel;

    public TweetHashtagService(IDatabase database, ISubscriber subscriber)
    {
        _database = database;
        _subscriber = subscriber;
        // AllHashtags = new ConcurrentStack<ScoredHashtag>();

        _amountToRankedHashtagsMap = new ConcurrentDictionary<int, ReplaySubject<RankedHashtag[]>>();

        _rankedHashtagChannel = new RedisChannel(HashtagConstants.RankedHashtagChannelName, RedisChannel.PatternMode.Literal);
        var rankedHashtagChannelMessageQueue = _subscriber.Subscribe(_rankedHashtagChannel);
        rankedHashtagChannelMessageQueue.OnMessage(OnRankedHashtagMessage);
    }

    public IObservable<RankedHashtag> GetRankedHashtagObservable(double samplingIntervalSeconds)
    {
        return _rankedHashtagStream.AsObservable().Sample(TimeSpan.FromSeconds(samplingIntervalSeconds));
    }

    public IObservable<RankedHashtag[]> GetRankedHashtagsObservable(int amount = 10)
    {
        var rankedHashtagsObservable = _amountToRankedHashtagsMap.GetOrAdd(amount, a => new ReplaySubject<RankedHashtag[]>(1));
        return rankedHashtagsObservable.AsObservable().Sample(TimeSpan.FromSeconds(5));
    }

    private void OnRankedHashtagMessage(ChannelMessage message)
    {
        string rankedHashtagStr = message.Message;
        var rankedHashtag = JsonConvert.DeserializeObject<RankedHashtag>(rankedHashtagStr);
        _rankedHashtagStream.OnNext(rankedHashtag);
    }

    private async Task ReplaceCurrentRankedHashtags(RedisKey previousKey, RankedHashtag[] currentRankedHashtags)
    {
        await _database.KeyDeleteAsync(previousKey).ConfigureAwait(false);
        await CacheCurrentRankedHashtags(previousKey, currentRankedHashtags).ConfigureAwait(false);
    }

    private async Task CacheCurrentRankedHashtags(RedisKey previousKey, RankedHashtag[] currentRankedHashtags)
    {
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

    public async Task<RankedHashtag[]> GetTopHashtags(int amount = 10)
    {
        var range = await _database.SortedSetRangeByRankWithScoresAsync(new RedisKey(HashtagConstants.RankedHashtagsSortedSetKey), 0, amount, Order.Descending).ConfigureAwait(false);
        return range.Select(entry => new RankedHashtag {Name = entry.Element.ToString(), Rank = entry.Score}).ToArray();
    }
}
