using GraphQL.Types;
using Visualizer.HostedServices;

namespace Visualizer.GraphQl.Queries;

public class StreamingQuery : ObjectGraphType
{
    public StreamingQuery(IServiceProvider provider)
    {
        var tweeterStreamingStarterService = provider.CreateScope().ServiceProvider.GetService<TweeterStreamingStarterService>();

        Field<BooleanGraphType>(
            "isStreaming",
            "Whether or not the live ingestion is running.",
            resolve: context => tweeterStreamingStarterService.IsStreaming
        );
    }
}