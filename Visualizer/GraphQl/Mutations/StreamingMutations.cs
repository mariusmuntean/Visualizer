using GraphQL.Types;
using Visualizer.HostedServices;

namespace Visualizer.GraphQl.Mutations;

public class StreamingMutations : ObjectGraphType
{
    public StreamingMutations(TweeterStreamingStarterService tweeterStreamingStarterService)
    {
        Field<BooleanGraphType>("startStreaming", "Start ingesting the live Twitter feed", resolve: context =>
        {
            tweeterStreamingStarterService.StartChecking();
            return true;
        });

        Field<BooleanGraphType>("stopStreaming", "Stop ingesting the live Twitter feed", resolve: context =>
        {
            tweeterStreamingStarterService.StopChecking();
            return true;
        });
    }
}