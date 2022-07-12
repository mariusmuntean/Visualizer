using System.Linq.Expressions;
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

    public async Task<List<TweetModel>> FindTweets(FindTweetsInputDto inputDto)
    {
        var tweetCollection = _redisConnectionProvider.RedisCollection<TweetModel>();

        if (!string.IsNullOrWhiteSpace(inputDto.TweetId))
        {
            tweetCollection = tweetCollection.Where(tweet => tweet.Id == inputDto.TweetId);
        }

        if (!string.IsNullOrWhiteSpace(inputDto.AuthorId))
        {
            tweetCollection = tweetCollection.Where(tweet => tweet.AuthorId == inputDto.AuthorId);
        }

        if (!string.IsNullOrWhiteSpace(inputDto.SearchTerm))
        {
            tweetCollection = tweetCollection.Where(tweet => tweet.Text == inputDto.SearchTerm);
        }

        if (inputDto.StartingFrom is not null)
        {
            var startingFromTicks = inputDto.StartingFrom.Value.ToUniversalTime().Ticks;
            tweetCollection = tweetCollection.Where(tweet => tweet.CreatedAt >= startingFromTicks);
        }

        if (inputDto.UpTo is not null)
        {
            var upToTicks = inputDto.UpTo.Value.ToUniversalTime().Ticks;
            tweetCollection = tweetCollection.Where(tweet => tweet.CreatedAt <= upToTicks);
        }

        if (inputDto.Hashtags is not null && inputDto.Hashtags.Length > 0)
        {
            foreach (var requiredHashtag in inputDto.Hashtags)
            {
                tweetCollection = tweetCollection.Where(tweet => tweet.Entities.Hashtags.Contains(requiredHashtag));
            }
        }

        var pageSize = inputDto.PageSize ?? 100;
        var pageNumber = inputDto.PageNumber ?? 0;
        tweetCollection = tweetCollection.Skip(pageSize * pageNumber).Take(pageSize);

        var tweetsIlist = await tweetCollection.ToListAsync();
        return tweetsIlist.ToList();
    }

    public async Task<List<TweetModel>> FindTweetsWithExpression(FindTweetsInputDto inputDto)
    {
        var tweetCollection = _redisConnectionProvider.RedisCollection<TweetModel>();

        Expression expression = null;

        if (!string.IsNullOrWhiteSpace(inputDto.TweetId))
        {
            Expression<Func<TweetModel, bool>> filterByTweetIdExpression = tweet => tweet.Id == inputDto.TweetId;
            expression = expression is null ? filterByTweetIdExpression.Body : Expression.AndAlso(expression, filterByTweetIdExpression.Body);
        }

        if (!string.IsNullOrWhiteSpace(inputDto.AuthorId))
        {
            Expression<Func<TweetModel, bool>> filterByAuthorIdExpression = tweet => tweet.AuthorId == inputDto.AuthorId;
            expression = expression is null ? filterByAuthorIdExpression.Body : Expression.AndAlso(expression, filterByAuthorIdExpression.Body);
        }

        if (!string.IsNullOrWhiteSpace(inputDto.Username))
        {
            Expression<Func<TweetModel, bool>> filterByUsernameExpression = tweet => tweet.Username == inputDto.Username;
            expression = expression is null ? filterByUsernameExpression.Body : Expression.AndAlso(expression, filterByUsernameExpression.Body);
        }

        if (!string.IsNullOrWhiteSpace(inputDto.SearchTerm))
        {
            Expression<Func<TweetModel, bool>> filterBySearchTerm = tweet => tweet.Text == inputDto.SearchTerm;
            expression = expression is null ? filterBySearchTerm.Body : Expression.AndAlso(expression, filterBySearchTerm.Body);
        }

        if (!string.IsNullOrWhiteSpace(inputDto.Username))
        {
            Expression<Func<TweetModel, bool>> filterByUsernameExpression = tweet => tweet.Username == inputDto.Username;
            expression = expression is null ? filterByUsernameExpression.Body : Expression.AndAlso(expression, filterByUsernameExpression.Body);
        }

        if (inputDto.StartingFrom is not null)
        {
            var startingFromTicks = inputDto.StartingFrom.Value.ToUniversalTime().Ticks;
            Expression<Func<TweetModel, bool>> filterByStartingFromExpression = tweet => tweet.CreatedAt >= startingFromTicks;
            expression = expression is null ? filterByStartingFromExpression.Body : Expression.AndAlso(expression, filterByStartingFromExpression.Body);
        }

        if (inputDto.UpTo is not null)
        {
            var upToTicks = inputDto.UpTo.Value.ToUniversalTime().Ticks;
            Expression<Func<TweetModel, bool>> filterByUpToExpression = tweet => tweet.CreatedAt <= upToTicks;
            expression = expression is null ? filterByUpToExpression.Body : Expression.AndAlso(expression, filterByUpToExpression.Body);
        }

        if (inputDto.Hashtags is not null && inputDto.Hashtags.Length > 0)
        {
            foreach (var requiredHashtag in inputDto.Hashtags)
            {
                Expression<Func<TweetModel, bool>> hashtagEx = tweet => tweet.Entities.Hashtags.Contains(requiredHashtag);
                expression = expression is null ? hashtagEx.Body : Expression.AndAlso(expression, hashtagEx.Body);
            }
        }

        if (expression is not null)
        {
            var tweetParameter = Expression.Parameter(typeof(TweetModel));
            var whereExpression = Expression.Lambda<Func<TweetModel, bool>>(expression, new ParameterExpression[] { tweetParameter });
            tweetCollection = tweetCollection.Where(whereExpression);
        }

        var sortField = inputDto.SortField ?? SortField.CreatedAt;
        var orderByDirection = inputDto.SortOrder ?? SortOrder.Descending;
        Expression<Func<TweetModel, String>> usernameKeySelector = tweet => tweet.Username;
        Expression<Func<TweetModel, long>> createdAtKeySelector = tweet => tweet.CreatedAt;
        tweetCollection = (orderByDirection, sortField) switch
        {
            (SortOrder.Ascending, SortField.Username) => tweetCollection.OrderBy(usernameKeySelector),
            (SortOrder.Ascending, SortField.CreatedAt) => tweetCollection.OrderBy(createdAtKeySelector),
            (SortOrder.Descending, SortField.Username) => tweetCollection.OrderByDescending(usernameKeySelector),
            (SortOrder.Descending, SortField.CreatedAt) => tweetCollection.OrderByDescending(createdAtKeySelector),
            _ => tweetCollection
        };

        var pageSize = inputDto.PageSize ?? 100;
        var pageNumber = inputDto.PageNumber ?? 0;
        tweetCollection = tweetCollection.Skip(pageSize * pageNumber).Take(pageSize);
        _logger.LogInformation("Generated Redis query {Query}", tweetCollection.Expression.ToString());

        var tweetsIlist = await tweetCollection.ToListAsync();
        return tweetsIlist.ToList();
    }
}

public class FindTweetsInputDto
{
    public string TweetId { get; set; }
    public string AuthorId { get; set; }
    public string Username { get; set; }
    public string SearchTerm { get; set; }
    public string[] Hashtags { set; get; }

    public int? PageSize { get; set; }
    public int? PageNumber { get; set; }

    public DateTime? StartingFrom { get; set; }
    public DateTime? UpTo { get; set; }

    public SortField? SortField { get; set; }
    public SortOrder? SortOrder { get; set; }
}

public enum SortField
{
    CreatedAt,
    Username,
}

public enum SortOrder
{
    Ascending,
    Descending
}
