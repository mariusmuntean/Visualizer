
using Redis.OM.Modeling;

namespace Visualizer.Shared.Models;

public class TweetEntities
{
    /// <summary>
    /// Cashtags found in the tweet. A cashtag is a company ticker symbol preceded by
    /// the U.S. dollar sign, e.g. $TWTR.
    /// </summary>
    // [Indexed]
    // public CashtagEntity[] Cashtags { get; set; }
    [Indexed]
    public string[] Cashtags { get; set; }

    /// <summary>
    /// Hashtags found in the tweet.
    /// </summary>
    // [Indexed]
    // public HashtagEntity[] Hashtags { get; set; }
    [Indexed]
    public string[] Hashtags { get; set; }

    /// <summary>
    ///  Mentions found in the tweet.
    /// </summary>
    // [Indexed]
    // public UserMention[] Mentions { get; set; }
    [Indexed]
    public string[] Mentions { get; set; }
}
