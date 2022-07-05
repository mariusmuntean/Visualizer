using GraphQL.Types;
using Visualizer.Model.TweetDb;

namespace Visualizer.GraphQl.Types.Tweet;

public class ReferencedTweetTypeQl : ObjectGraphType<ReferencedTweet>
{
    public ReferencedTweetTypeQl()
    {
        Field(rt => rt.Id, false, typeof(StringGraphType));
        Field(rt => rt.Type, false, typeof(StringGraphType));
    }
}
