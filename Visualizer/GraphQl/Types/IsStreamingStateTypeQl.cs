using GraphQL.Types;
using Visualizer.HostedServices;

namespace Visualizer.GraphQl.Types;

public class IsStreamingStateTypeQl : ObjectGraphType<TweeterStreamingStarterService.IsStreamingState>
{
    public IsStreamingStateTypeQl()
    {
        Field(entry => entry.IsStreaming, false, typeof(BooleanGraphType));
    }
}
