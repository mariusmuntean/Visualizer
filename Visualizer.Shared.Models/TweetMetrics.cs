
using Redis.OM.Modeling;

namespace Visualizer.Shared.Models;

public class TweetMetrics
{
    [Indexed]
    public int ImpressionCount { get; set; }

    [Indexed]
    public int LikeCount { get; set; }

    [Indexed]
    public int ReplyCount { get; set; }

    [Indexed]
    public int RetweetCount { get; set; }

    [Indexed]
    public int UrlLinkClicks { get; set; }

    [Indexed]
    public int UserProfileClicks { get; set; }
}
