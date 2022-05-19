using GraphQL.Types;
using StackExchange.Redis;

namespace Visualizer.GraphQl.Types;

public class HashtagTypeQl : ObjectGraphType<SortedSetEntry>
{
    public HashtagTypeQl()
    {
        Field(entry => entry.Score, false, typeof(DecimalGraphType));
        Field("Name",entry => entry.Element.ToString(), false, typeof(StringGraphType));
    }
}