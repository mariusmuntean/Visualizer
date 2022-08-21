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

        Field<BooleanGraphType>("stopStreaming", "Stop ingesting the live Twitter feed", resolve: context =>
        {
            var _ = ingestionServiceProxy.StopStreaming();
            return true;
        });
    }
}
