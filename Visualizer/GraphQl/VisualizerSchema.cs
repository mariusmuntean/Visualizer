using GraphQL.Types;
using Visualizer.GraphQl.Queries;
using Visualizer.GraphQl.Subscriptions;

namespace Visualizer.GraphQl;

public class VisualizerSchema : Schema
{
    public VisualizerSchema(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.Query = serviceProvider.GetRequiredService<VisualizerQuery>();
        // this.Subscription = serviceProvider.GetRequiredService<VisualizerSubscription>();
        this.Subscription = new HashtagSubscription(serviceProvider);
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
    
    // public class VisualizerSubscription : ObjectGraphType
    // {
    //     public VisualizerSubscription()
    //     {
    //         Name = nameof(VisualizerSubscription);
    //         Field<HashtagSubscription>("hashtagSubscription", resolve: context => new { });
    //     }
    // }
}