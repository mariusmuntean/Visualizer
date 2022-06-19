using NRedisGraph;
using Visualizer.Services.Ingestion;

namespace Visualizer.Services.Extensions;

public static class NodeExtensions
{
    public static TweetGraphService.UserNode ToUserNode(this Node node)
    {
        return new TweetGraphService.UserNode(
            node.PropertyMap[nameof(TweetGraphService.UserNode.UserId)].Value.ToString() ?? throw new InvalidOperationException(),
            node.PropertyMap[nameof(TweetGraphService.UserNode.UserName)].Value.ToString() ?? throw new InvalidOperationException()
        );
    }
}

public static class EdgeExtensions
{
    public static TweetGraphService.MentionRelationship ToMentionRelationship(this Edge edge, string fromUserId, string touserid)
    {
        return new TweetGraphService.MentionRelationship(
            fromUserId,
            touserid,
            edge.PropertyMap[nameof(TweetGraphService.MentionRelationship.TweetId)].Value.ToString() ?? throw new InvalidOperationException(),
            Enum.Parse<TweetGraphService.MentionRelationshipType>(edge.RelationshipType, ignoreCase: true)
        );
    }
}