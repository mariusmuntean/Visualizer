
using Redis.OM.Modeling;

namespace Visualizer.Model.TweetDb;

public class ReferencedTweet
{
    /// <summary>
    /// The unique identifier of the referenced Tweet.
    /// </summary>
    [Indexed]
    public string Id { get; set; }

    /// <summary>
    /// Indicates the type of relationship between this Tweet and the Tweet returned
    /// in the response: * retweeted (this Tweet is a Retweet), * quoted (a Retweet with
    /// comment, also known as Quoted Tweet), * or replied_to (this Tweet is a reply).
    /// </summary>
    [Indexed(Sortable = true)]
    public string Type { get; set; }
}
