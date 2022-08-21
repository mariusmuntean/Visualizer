using GraphQL.Types;
using Visualizer.API.GraphQl.Mutations;

namespace Visualizer.API.GraphQl;

public class VisualizerMutation : ObjectGraphType
{
    public VisualizerMutation()
    {
        Name = nameof(VisualizerMutation);
        Field<StreamingMutations>("streaming", resolve: context => new { });
    }
}
