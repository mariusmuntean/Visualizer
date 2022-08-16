using GraphQL.Types;
using Visualizer.API.GraphQl.Mutations;
using Visualizer.API.GraphQl.Queries;

namespace Visualizer.API.GraphQl;

public class VisualizerSchema : Schema
{
    public VisualizerSchema(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Query = serviceProvider.GetRequiredService<VisualizerQuery>();
        Mutation = serviceProvider.GetRequiredService<VisualizerMutation>();
        // Subscription = serviceProvider.GetRequiredService<VisualizerSubscription>();
        Subscription = new Subscriptions.VisualizerSubscription(serviceProvider);
    }

    // The following classes aren't explicitly added to the DI container, but they're still there. The "AddGraphTypes(...)" takes care of this.

    public class VisualizerQuery : ObjectGraphType
    {
        public VisualizerQuery()
        {
            Name = nameof(VisualizerQuery);
            Field<TweetQuery>("tweet", resolve: context => new { });
            Field<HashtagQuery>("hashtag", resolve: context => new { });
            Field<GraphResultQuery>("graphResult", resolve: context => new { });
            Field<StreamingQuery>("streaming", resolve: context => new { });
        }
    }

    public class VisualizerMutation : ObjectGraphType
    {
        public VisualizerMutation()
        {
            Name = nameof(VisualizerMutation);
            Field<StreamingMutations>("streaming", resolve: context => new { });
        }
    }

    // It seems to be a limitation of our graphql lib that forces us to have all the subscriptions in a single class, instead of the normal hierarchical organization.
    public class VisualizerSubscription : ObjectGraphType
    {
        public VisualizerSubscription()
        {
            Name = nameof(VisualizerSubscription);
            Field<Subscriptions.VisualizerSubscription>("subscriptions", resolve: context => new { });
        }
    }
}
