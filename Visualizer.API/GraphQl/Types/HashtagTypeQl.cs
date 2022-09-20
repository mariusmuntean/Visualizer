using GraphQL.Types;
using Visualizer.API.Services.DTOs;
using Visualizer.Shared.Models;

namespace Visualizer.API.GraphQl.Types;

public class HashtagTypeQl : ObjectGraphType<RankedHashtag>
{
    public HashtagTypeQl()
    {
        Field(entry => entry.Name, false, typeof(StringGraphType));
        Field(entry => entry.Rank, false, typeof(DecimalGraphType));

        // ToDo: add another typeQl class for GraphResult
    }
}
