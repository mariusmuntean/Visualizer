using GraphQL.Types;
using Visualizer.Shared.Models;

namespace Visualizer.API.GraphQl.Types.Graph;

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