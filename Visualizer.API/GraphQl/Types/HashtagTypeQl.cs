using GraphQL.Types;
using Visualizer.API.Services.DTOs;

namespace Visualizer.API.GraphQl.Types;

public class HashtagTypeQl : ObjectGraphType<ScoredHashtag>
{
    public HashtagTypeQl()
    {
        Field(entry => entry.Name, false, typeof(StringGraphType));
        Field(entry => entry.Score, false, typeof(DecimalGraphType));

        // ToDo: add another typeQl class for GraphResult
    }
}
