using GraphQL.Types;
using Visualizer.Shared.Models;

namespace Visualizer.API.GraphQl.Types.Tweet;

public class TweetEntitiesTypeQl : ObjectGraphType<TweetEntities>
{
    public TweetEntitiesTypeQl()
    {
        Field(e => e.Hashtags, true, typeof(ListGraphType<StringGraphType>));
        Field(e => e.Cashtags, true, typeof(ListGraphType<CashtagEntityTypeQl>));
        Field(e => e.Mentions, true, typeof(ListGraphType<StringGraphType>));
    }
}
