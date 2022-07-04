
using Redis.OM.Modeling;

namespace Visualizer.Model.TweetDb;

public class TweetEntities
{
    /// <summary>
    /// Cashtags found in the tweet. A cashtag is a company ticker symbol preceded by
    /// the U.S. dollar sign, e.g. $TWTR.
    /// </summary>
    [Indexed(CascadeDepth = 1)]
    public CashtagEntity[] Cashtags { get; set; }

    /// <summary>
    /// Hashtags found in the tweet.
    /// </summary>
    [Indexed(CascadeDepth = 1)]
    public HashtagEntity[] Hashtags { get; set; }

    /// <summary>
    ///  Mentions found in the tweet.
    /// </summary>
    [Indexed(CascadeDepth = 1)]
    public UserMention[] Mentions { get; set; }
}
