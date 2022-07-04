using Microsoft.Extensions.Logging;
using Redis.OM;
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
        var queryableTweetCollection = tweetCollection.AsQueryable();

        var pageSize = inputDto.PageSize ?? 100;
        var pageNumber = inputDto.PageNumber ?? 0;
        queryableTweetCollection = queryableTweetCollection.Skip(pageSize * pageNumber);
        queryableTweetCollection = queryableTweetCollection.Take(pageSize);

        if (inputDto.TweetId is not null)
        {
            queryableTweetCollection = queryableTweetCollection.Where(tweet => tweet.Id == inputDto.TweetId);
        }

        if (inputDto.AuthorId is not null)
        {
            queryableTweetCollection = queryableTweetCollection.Where(tweet => tweet.AuthorId == inputDto.AuthorId);
        }

        if (inputDto.StartingFrom is not null)
        {
            queryableTweetCollection = queryableTweetCollection.Where(tweet => tweet.CreatedAt >= inputDto.StartingFrom);
        }

        if (inputDto.UpTo is not null)
        {
            queryableTweetCollection = queryableTweetCollection.Where(tweet => tweet.CreatedAt <= inputDto.UpTo);
        }

        if (inputDto.Hashtags is not null)
        {
            // queryableTweetCollection = queryableTweetCollection.Where(tweet => tweet.Entities.Hashtags.Contains());
        }

        if (inputDto.SearchTerm is not null)
        {
            queryableTweetCollection = queryableTweetCollection.Where(tweet => tweet.Text == inputDto.SearchTerm);
        }

        return queryableTweetCollection.ToList();
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
