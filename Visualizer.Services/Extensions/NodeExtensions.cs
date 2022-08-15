using NRedisGraph;
using Visualizer.Shared.Models;

namespace Visualizer.Services.Extensions;

public static class NodeExtensions
{
    public static UserNode ToUserNode(this Node node)
    {
        return new UserNode(
            node.PropertyMap[nameof(UserNode.UserId)].Value.ToString() ?? throw new InvalidOperationException(),
            node.PropertyMap[nameof(UserNode.UserName)].Value.ToString() ?? throw new InvalidOperationException()
        );
    }
}

public static class EdgeExtensions
{
    public static MentionRelationship ToMentionRelationship(this Edge edge, string fromUserId, string touserid)
    {
        return new MentionRelationship(
            fromUserId,
            touserid,
            edge.PropertyMap[nameof(MentionRelationship.TweetId)].Value.ToString() ?? throw new InvalidOperationException(),
            Enum.Parse<MentionRelationshipType>(edge.RelationshipType, ignoreCase: true)
        );
    }
}
