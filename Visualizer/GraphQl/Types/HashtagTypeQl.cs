using GraphQL.Types;
using Visualizer.Services;
using Visualizer.Services.Ingestion;

namespace Visualizer.GraphQl.Types;

public class HashtagTypeQl : ObjectGraphType<TweetHashtagService.ScoredHashtag>
{
    public HashtagTypeQl()
    {
        Field(entry => entry.Name, false, typeof(StringGraphType));
        Field(entry => entry.Score, false, typeof(DecimalGraphType));

        // ToDo: add another typeQl class for GraphResult
    }
}