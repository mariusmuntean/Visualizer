using GraphQL.Types;
using Visualizer.API.Services.Services;

namespace Visualizer.API.GraphQl.Mutations;

public class StreamingMutations : ObjectGraphType
{
    public StreamingMutations(IIngestionServiceProxy ingestionServiceProxy)
    {
        FieldAsync<BooleanGraphType>("startStreaming", "Start ingesting the live Twitter feed", resolve: async context =>
        {
            var hasStartedStreaming = await ingestionServiceProxy.StartStreaming().ConfigureAwait(false);
            return true;
        });

        FieldAsync<BooleanGraphType>("stopStreaming", "Stop ingesting the live Twitter feed", resolve: async context =>
        {
            var _ = await ingestionServiceProxy.StopStreaming().ConfigureAwait(false);
            return true;
        });
    }
}
