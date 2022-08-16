using GraphQL.Types;
using Visualizer.API.Model.TweetDb;

namespace Visualizer.API.GraphQl.Types.Tweet;

public class UserMentionTypeQl : ObjectGraphType<UserMention>
{
    public UserMentionTypeQl()
    {
        Field(h => h.Username, false, typeof(StringGraphType));
        Field(h => h.Start, false, typeof(IntGraphType));
        Field(h => h.End, false, typeof(IntGraphType));
    }
}
