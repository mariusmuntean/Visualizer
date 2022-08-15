using GraphQL.Types;
using Visualizer.API.Model.TweetDb;

namespace Visualizer.GraphQl.Types.Tweet;

public class CashtagEntityTypeQl : ObjectGraphType<CashtagEntity>
{
    public CashtagEntityTypeQl()
    {
        Field(h => h.Tag, false, typeof(StringGraphType));
        Field(h => h.Start, false, typeof(IntGraphType));
        Field(h => h.End, false, typeof(IntGraphType));
    }
}
