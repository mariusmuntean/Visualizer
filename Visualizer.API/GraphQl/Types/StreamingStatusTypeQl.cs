using GraphQL.Types;
using Visualizer.Shared.Models;

namespace Visualizer.API.GraphQl.Types;

public class StreamingStatusTypeQl : ObjectGraphType<StreamingStatusDto>
{
    public StreamingStatusTypeQl()
    {
        Field(entry => entry.IsStreaming, false, typeof(BooleanGraphType));
    }
}
