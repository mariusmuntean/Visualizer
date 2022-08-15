using GraphQL.Types;
using Visualizer.Services.Ingestion;

namespace Visualizer.GraphQl.Mutations;

public class StreamingMutations : ObjectGraphType
{
    public StreamingMutations(IIngestionService ingestionService)
    {
        FieldAsync<BooleanGraphType>("startStreaming", "Start ingesting the live Twitter feed", resolve: async context =>
        {
            var hasStartedStreaming = await ingestionService.StartStreaming().ConfigureAwait(false);
            return true;
        });

        Field<BooleanGraphType>("stopStreaming", "Stop ingesting the live Twitter feed", resolve: context =>
        {
            var _ = ingestionService.StopStreaming();
            return true;
        });
    }
}
