using GraphQL.Types;
using Visualizer.API.Services.DTOs;

namespace Visualizer.API.GraphQl.Types.Graph;

public class GraphResultStatisticsTypeQl : ObjectGraphType<GraphResultStatisticsDto>
{
    public GraphResultStatisticsTypeQl()
    {
        Field(nameof(GraphResultStatisticsDto.QueryInternalExecutionTime), stats => stats.QueryInternalExecutionTime, nullable: true, typeof(StringGraphType));
    }
}