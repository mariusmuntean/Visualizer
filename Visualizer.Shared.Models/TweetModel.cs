using Redis.OM.Modeling;

namespace Visualizer.Shared.Models;

[Document(StorageType = StorageType.Json, Prefixes = new[] { nameof(TweetModel) })]
public class TweetModel
{
    [RedisIdField]
    [Indexed]
    public string? InternalId { get; set; }

    [Indexed]
    public string Id { get; set; }

    /// <summary>
    /// The Tweet ID of the original Tweet of the conversation (which includes direct replies, replies of replies).
    /// </summary>
    [Indexed]
    public string ConversationId { get; set; }

    /// <summary>
    /// The actual UTF-8 text of the Tweet.
    /// </summary>
    [Searchable(PhoneticMatcher = "dm:en")] // src https://github.com/redis/redis-om-node/blob/main/README.md
    public string Text { get; set; }

    [Indexed]
    public string AuthorId { get; set; }

    [Indexed]
    public string Username { get; set; }

    /// <summary>
    /// UTC ticks when the Tweet was created.
    /// </summary>
    /// <value></value>
    [Indexed(Sortable = true, Aggregatable = true)]
    public long CreatedAt { get; set; }

    /// <summary>
    /// Contains details about the location tagged by the user in this Tweet, if they specified one.
    /// </summary>
    [Indexed(Aggregatable = true)]
    public GeoLoc? GeoLoc { get; set; }

    [Indexed] // src https://github.com/redis/redis-om-node/blob/main/README.md
    public string HasGeoLoc { get; set; }

    /// <summary>
    /// Language of the Tweet, if detected by Twitter. Returned as a BCP47 language tag.
    /// </summary>
    [Indexed]
    public string Lang { get; set; }

    /// <summary>
    ///  The name of the app the user Tweeted from.
    /// </summary>
    [Searchable]
    public string Source { get; set; }

    /// <summary>
    /// Entities which have been parsed out of the text of the Tweet.
    /// </summary>
    [Indexed(CascadeDepth = 2)]
    public TweetEntities Entities { get; set; }

    /// <summary>
    /// Engagement metrics, tracked in an organic context, for the Tweet at the time of the request.
    /// </summary>
    [Indexed(CascadeDepth = 1)]
    public TweetMetrics OrganicMetrics { get; set; }

    // /// <summary>
    // /// Public engagement metrics for the Tweet at the time of the request.
    // /// </summary>
    //[Indexed(CascadeDepth = 2)]
    //public TweetPublicMetrics PublicMetrics { get; set; }
    [Indexed]
    public int PublicMetricsLikeCount { get; set; }

    [Indexed]
    public int PublicMetricsQuoteCount { get; set; }

    [Indexed]
    public int PublicMetricsReplyCount { get; set; }

    [Indexed]
    public int PublicMetricsRetweetCount { get; set; }

    /// <summary>
    /// A list of Tweets this Tweet refers to. For example, if the parent Tweet is a Retweet, a Retweet with comment (also known as Quoted Tweet) or a Reply, it will include the related Tweet referenced to by its parent.
    /// </summary>
    [Indexed(CascadeDepth = 1)]
    public ReferencedTweet[] ReferencedTweets { get; set; }
}
