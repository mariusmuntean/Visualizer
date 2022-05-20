using GraphQL.Types;
using Visualizer.GraphQl.Queries;

namespace Visualizer.GraphQl;

public class VisualizerSchema : Schema
{
    public VisualizerSchema(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Query = serviceProvider.GetRequiredService<VisualizerQuery>();
    }

    public class VisualizerQuery : ObjectGraphType
    {
        public VisualizerQuery()
        {
            Name = nameof(VisualizerQuery);
            Field<HashtagQuery>("hashtag", resolve: context => new { });
            Field<GraphResultQuery>("graphResult", resolve: context => new { });
        }
    }
}