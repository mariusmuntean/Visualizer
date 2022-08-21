using GraphQL.Types;

namespace Visualizer.API.GraphQl;

/// <summary>
/// It seems to be a limitation of our graphql lib that forces us to have all the subscriptions in a single class, instead of the normal hierarchical organization.
/// </summary>
public class VisualizerSubscription : ObjectGraphType
{
    public VisualizerSubscription()
    {
        Name = nameof(VisualizerSubscription);
        Field<Subscriptions.VisualizerSubscription>("subscriptions", resolve: context => new { });
    }
}
