using GraphQL.Types;
using Visualizer.Services.Ingestion;

namespace Visualizer.GraphQl.Types;

public class GraphResultTypeQl : ObjectGraphType<TweetGraphService.GraphResult>
{
    public GraphResultTypeQl()
    {
        Field(nameof(TweetGraphService.GraphResult.Nodes),
            result => result.Nodes.Values.ToArray(),
            false, typeof(ListGraphType<UserNodeTypeQl>));

        Field(nameof(TweetGraphService.GraphResult.Edges),
            result => result.Edges.ToArray(),
            false, typeof(ListGraphType<MentionRelationshipTypeQl>));
    }
}

public class UserNodeTypeQl : ObjectGraphType<TweetGraphService.UserNode>
{
    public UserNodeTypeQl()
    {
        Field(node => node.UserId, false);
        Field(node => node.UserName, true);
    }
}

public class MentionRelationshipTypeQl : ObjectGraphType<TweetGraphService.MentionRelationship>
{
    public MentionRelationshipTypeQl()
    {
        Field(graphEdge => graphEdge.FromUserId, false);
        Field(graphEdge => graphEdge.ToUserId, false);
        Field(graphEdge => graphEdge.TweetId, false);
        Field(graphEdge => graphEdge.RelationshipType, false, typeof(EnumerationGraphType<TweetGraphService.MentionRelationshipType>));
    }
}