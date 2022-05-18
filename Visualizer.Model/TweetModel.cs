using Redis.OM.Modeling;

namespace Visualizer.Model;

[Document(StorageType = StorageType.Json, Prefixes = new[] { nameof(TweetModel) })]
public class TweetModel
{
    [RedisIdField]
    [Indexed]
    public string? InternalId { get; set; }

    [Indexed]
    public string Id { get; set; }

    [Searchable]
    public string Text { get; set; }

    [Indexed]
    public string AuthorId { get; set; }

    [Indexed]
    public DateTime CreatedAt { get; set; }
}