using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Redis.OM;
using Redis.OM.Searching;
using Visualizer.Model.TweetDb;

namespace Visualizer.Services.Query;

public class TweetDbQueryService
{

    private readonly RedisConnectionProvider _redisConnectionProvider;
    private readonly ILogger<TweetDbQueryService> _logger;

    public TweetDbQueryService(RedisConnectionProvider redisConnectionProvider, ILogger<TweetDbQueryService> logger)
    {
        _redisConnectionProvider = redisConnectionProvider;
        _logger = logger;
    }

    public List<TweetModel> FindTweets(FindTweetsInputDto inputDto)
    {
        var tweetCollection = _redisConnectionProvider.RedisCollection<TweetModel>();

        var pageSize = inputDto.PageSize ?? 100;
        var pageNumber = inputDto.PageNumber ?? 0;
        tweetCollection = tweetCollection.Skip(pageSize * pageNumber);
        tweetCollection = tweetCollection.Take(pageSize);

        if (inputDto.TweetId is not null)
        {
            tweetCollection = tweetCollection.Where(tweet => tweet.Id == inputDto.TweetId);
        }

        if (inputDto.AuthorId is not null)
        {

            // ToDo: build expression like here https://stackoverflow.com/a/8315901
            tweetCollection = tweetCollection.Where(tweet => tweet.AuthorId == inputDto.AuthorId && tweet.Text == inputDto.SearchTerm);
            // ToDo #2: open issue about using multiple Where clauses

            // }

            // if (inputDto.SearchTerm is not null)
            // {
            // tweetCollection = tweetCollection.Where(tweet => tweet.Text == inputDto.SearchTerm);
        }

        if (inputDto.StartingFrom is not null)
        {
            tweetCollection = tweetCollection.Where(tweet => tweet.CreatedAt >= inputDto.StartingFrom);
        }

        if (inputDto.UpTo is not null)
        {
            tweetCollection = tweetCollection.Where(tweet => tweet.CreatedAt <= inputDto.UpTo);
        }

        if (inputDto.Hashtags is not null && inputDto.Hashtags.Length > 0)
        {
            // workaround until the PR is merged https://github.com/redis/redis-om-dotnet/pull/151

            foreach (var requiredHashtag in inputDto.Hashtags)
            {

                tweetCollection = tweetCollection.Where(tweet => tweet.Entities.Hashtags.Contains(requiredHashtag));
            }
        }

        return tweetCollection.ToList();
    }
}

public class FindTweetsInputDto
{
    public int? PageSize { get; set; }
    public int? PageNumber { get; set; }

    public string TweetId { get; set; }
    public string AuthorId { get; set; }

    public DateTime? StartingFrom { get; set; }
    public DateTime? UpTo { get; set; }

    public string[] Hashtags { get; set; }

    public string SearchTerm { get; set; }
}
