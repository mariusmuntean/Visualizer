using GraphQL.Types;
using Visualizer.API.Services.Ingestion;
using Visualizer.Shared.Models;
using static Visualizer.API.Services.Ingestion.TweetGraphService;

namespace Visualizer.GraphQl.Types;

public class GraphResultTypeQl : ObjectGraphType<GraphResult>
{
    public GraphResultTypeQl()
    {
        Field(nameof(TweetGraphService.GraphResult.Nodes),
            result => result.Nodes.Values.ToArray(),
            false, typeof(ListGraphType<UserNodeTypeQl>));

        Field(nameof(TweetGraphService.GraphResult.Edges),
            result => result.Edges.ToArray(),
            false, typeof(ListGraphType<MentionRelationshipTypeQl>));

        Field(result => result.Statistics, true, typeof(GraphResultStatisticsTypeQl));
    }
}

public class GraphResultStatisticsTypeQl : ObjectGraphType<GraphResultStatistics>
{
    public GraphResultStatisticsTypeQl()
    {
        Field(nameof(GraphResultStatistics.QueryInternalExecutionTime), stats => stats.QueryInternalExecutionTime, nullable: true, typeof(StringGraphType));
    }
}

public class UserNodeTypeQl : ObjectGraphType<UserNode>
{
    public UserNodeTypeQl()
    {
        Field(node => node.UserId, false);
        Field(node => node.UserName, true);
    }
}

public class MentionRelationshipTypeQl : ObjectGraphType<MentionRelationship>
{
    public MentionRelationshipTypeQl()
    {
        Field(graphEdge => graphEdge.FromUserId, false);
        Field(graphEdge => graphEdge.ToUserId, false);
        Field(graphEdge => graphEdge.TweetId, false);
        Field(graphEdge => graphEdge.RelationshipType, false, typeof(EnumerationGraphType<MentionRelationshipType>));
    }
}
