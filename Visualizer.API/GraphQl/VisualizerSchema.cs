using GraphQL.Types;

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
}
