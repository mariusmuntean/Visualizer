using GraphQL.Types;
using Visualizer.API.Services.Services;

namespace Visualizer.API.GraphQl.Queries;

public class StreamingQuery : ObjectGraphType
{
    public StreamingQuery(IServiceProvider provider)
    {
        var ingestionService = provider.CreateScope().ServiceProvider.GetService<IIngestionServiceProxy>();

        Field<BooleanGraphType>("isStreaming")
            .Description("Whether or not the live ingestion is running.")
            .ResolveAsync(async ctx => await ingestionService.IsStreaming().ConfigureAwait(false));
    }
}
