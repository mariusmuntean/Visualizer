using GraphQL.Types;
using Visualizer.HostedServices;

namespace Visualizer.GraphQl.Queries;

public class StreamingQuery : ObjectGraphType
{
    public StreamingQuery(IServiceProvider provider)
    {
        var ingestionService = provider.CreateScope().ServiceProvider.GetService<IngestionService>();

        Field<BooleanGraphType>(
            "isStreaming",
            "Whether or not the live ingestion is running.",
            resolve: context => ingestionService.IsStreaming
        );
    }
}
