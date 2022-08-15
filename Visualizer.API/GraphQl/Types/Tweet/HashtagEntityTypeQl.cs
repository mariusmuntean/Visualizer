using GraphQL.Types;
using Visualizer.API.Model.TweetDb;

namespace Visualizer.GraphQl.Types.Tweet;

public class HashtagEntityTypeQl : ObjectGraphType<HashtagEntity>
{
    public HashtagEntityTypeQl()
    {
        Field(h => h.Hashtag, false, typeof(StringGraphType));
        Field(h => h.Start, false, typeof(IntGraphType));
        Field(h => h.End, false, typeof(IntGraphType));
    }
}
