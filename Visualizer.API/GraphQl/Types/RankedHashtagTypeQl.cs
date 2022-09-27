using GraphQL.Types;
using Visualizer.Shared.Models;

namespace Visualizer.API.GraphQl.Types;

public class RankedHashtagTypeQl : ObjectGraphType<RankedHashtag>
{
    public RankedHashtagTypeQl()
    {
        Field(entry => entry.Name, false, typeof(StringGraphType));
        Field(entry => entry.Rank, false, typeof(DecimalGraphType));

        // ToDo: add another typeQl class for GraphResult
    }
}
