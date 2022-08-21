using GraphQL.Types;
using Visualizer.API.Services.DTOs;

namespace Visualizer.API.GraphQl.Types.Graph;

public class GraphResultTypeQl : ObjectGraphType<GraphResultDto>
{
    public GraphResultTypeQl()
    {
        Field(nameof(GraphResultDto.Nodes),
            result => result.Nodes.Values.ToArray(),
            false, typeof(ListGraphType<UserNodeTypeQl>));

        Field(nameof(GraphResultDto.Edges),
            result => result.Edges.ToArray(),
            false, typeof(ListGraphType<MentionRelationshipTypeQl>));

        Field(result => result.Statistics, true, typeof(GraphResultStatisticsTypeQl));
    }
}
