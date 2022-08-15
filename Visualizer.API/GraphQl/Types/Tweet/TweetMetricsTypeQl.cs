using GraphQL.Types;
using Visualizer.Shared.Models;

namespace Visualizer.GraphQl.Types.Tweet;

public class TweetMetricsTypeQl : ObjectGraphType<TweetMetrics>
{
    public TweetMetricsTypeQl()
    {
        Field(m => m.ImpressionCount, true, typeof(IntGraphType));
        Field(m => m.LikeCount, true, typeof(IntGraphType));
        Field(m => m.ReplyCount, true, typeof(IntGraphType));
        Field(m => m.RetweetCount, true, typeof(IntGraphType));
        Field(m => m.UrlLinkClicks, true, typeof(IntGraphType));
        Field(m => m.UserProfileClicks, true, typeof(IntGraphType));
    }
}
