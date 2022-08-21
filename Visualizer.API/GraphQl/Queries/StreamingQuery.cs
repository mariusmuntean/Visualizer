using GraphQL.Types;
using Visualizer.API.Services.Services;

namespace Visualizer.API.GraphQl.Queries;

public class StreamingQuery : ObjectGraphType
{
    public StreamingQuery(IServiceProvider provider)
    {
        var ingestionService = provider.CreateScope().ServiceProvider.GetService<IIngestionServiceProxy>();

        Field<BooleanGraphType>("isStreaming",
            "Whether or not the live ingestion is running.",
            resolve: context => ingestionService.IsStreaming
        );
    }
}
