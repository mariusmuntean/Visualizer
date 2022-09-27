namespace Visualizer.Shared.Models;

public record MentionRelationship(string FromUserId, string ToUserId, string TweetId, MentionRelationshipType RelationshipType);
