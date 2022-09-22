using GraphQL.Types;
using Visualizer.API.GraphQl.Queries;

namespace Visualizer.API.GraphQl;

public class VisualizerQuery : ObjectGraphType
{
    public VisualizerQuery()
    {
        Name = nameof(VisualizerQuery);
        Field<StreamingQuery>("streaming", resolve: context => new { });
        Field<RankedHashtagQuery>("hashtag", resolve: context => new { });
        Field<TweetQuery>("tweet", resolve: context => new { });
        Field<GraphResultQuery>("graphResult", resolve: context => new { });
    }
}
