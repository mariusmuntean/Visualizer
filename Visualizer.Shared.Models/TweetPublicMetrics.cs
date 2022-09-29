using Redis.OM.Modeling;

namespace Visualizer.Shared.Models;

public class TweetPublicMetrics
{
    [Indexed]
    public int LikeCount { get; set; }

    [Indexed]
    public int QuoteCount { get; set; }

    [Indexed]
    public int ReplyCount { get; set; }

    [Indexed]
    public int RetweetCount { get; set; }
}
