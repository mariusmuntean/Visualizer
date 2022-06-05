using System.Reactive.Linq;
using System.Reactive.Subjects;
using StackExchange.Redis;
using Tweetinvi.Events.V2;

namespace Visualizer.Services.Ingestion;

public class TweetHashtagService
{
    private const string HASHTAGS = "hashtags";

    private readonly IDatabase _database;
    private readonly ISubject<ScoredHashtag> _hashtagStream = new ReplaySubject<ScoredHashtag>(1);

    public TweetHashtagService(IDatabase database)
    {
        _database = database;
        // AllHashtags = new ConcurrentStack<ScoredHashtag>();
    }

    public async Task AddHashtags(TweetV2ReceivedEventArgs tweetV2ReceivedEventArgs)
    {
        if (!tweetV2ReceivedEventArgs.Tweet?.Entities?.Hashtags?.Any() ?? false)
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

            var sortedSetEntry = new ScoredHashtag { Name = hashtag, Score = newScore};
            // AllHashtags.Push(sortedSetEntry);
            _hashtagStream.OnNext(sortedSetEntry);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Failed to add/increment {hashtag} in the sorted set {HASHTAGS}. {ex.StackTrace}");
        }
    }

    public IObservable<ScoredHashtag> Hashtags() => _hashtagStream.AsObservable();
    // public ConcurrentStack<ScoredHashtag> AllHashtags { get; }

    public async Task<ScoredHashtag[]> GetTopHashtags(int amount = 10)
    {
        var range = await _database.SortedSetRangeByRankWithScoresAsync(new RedisKey(HASHTAGS), 0, amount, Order.Descending);
        return range.Select(entry => new ScoredHashtag{Name = entry.Element.ToString(), Score = entry.Score}).ToArray();
    }

    public class ScoredHashtag
    {
        public string Name { get; set; }
        public double Score { get; set; }
    }
}