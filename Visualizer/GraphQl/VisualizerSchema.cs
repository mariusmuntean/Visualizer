using GraphQL.Types;
using Visualizer.GraphQl.Queries;

namespace Visualizer.GraphQl;

public class VisualizerSchema : Schema
{
    public VisualizerSchema(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Query = serviceProvider.GetRequiredService<HashtagQuery>();
    }
}