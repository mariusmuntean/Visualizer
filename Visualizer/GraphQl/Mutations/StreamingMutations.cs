using GraphQL.Types;
using Visualizer.HostedServices;

namespace Visualizer.GraphQl.Mutations;

public class StreamingMutations : ObjectGraphType
{
    public StreamingMutations(TweeterStreamingStarterService tweeterStreamingStarterService)
    {
        FieldAsync<BooleanGraphType>("startStreaming", "Start ingesting the live Twitter feed", resolve: async context =>
        {
            var hasStartedStreaming = await tweeterStreamingStarterService.StartChecking();
            return hasStartedStreaming;
        });

        Field<BooleanGraphType>("stopStreaming", "Stop ingesting the live Twitter feed", resolve: context =>
        {
            tweeterStreamingStarterService.StopChecking();
            return true;
        });
    }
}
