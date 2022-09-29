using GraphQL.Types;
using Visualizer.Shared.Models;

namespace Visualizer.API.GraphQl.Types.Tweet;

public class TweetPublicMetricsTypeQl : ObjectGraphType<TweetPublicMetrics>
{
    public TweetPublicMetricsTypeQl()
    {
        Field(m => m.LikeCount, true, typeof(IntGraphType));
        Field(m => m.QuoteCount, true, typeof(IntGraphType));
        Field(m => m.ReplyCount, true, typeof(IntGraphType));
        Field(m => m.RetweetCount, true, typeof(IntGraphType));
    }
}
