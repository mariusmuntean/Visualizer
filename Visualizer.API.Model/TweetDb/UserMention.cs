using Redis.OM.Modeling;

namespace Visualizer.API.Model.TweetDb;

public class UserMention
{
    /// <summary>
    /// The end position (zero-based) of the recognized user mention within the Tweet.
    /// </summary>
    [Indexed]
    public int End { get; set; }

    /// <summary>
    /// The start position (zero-based) of the recognized user mention within the Tweet.
    /// </summary>
    [Indexed]
    public int Start { get; set; }

    /// <summary>
    /// The part of text recognized as a user mention.
    /// </summary>
    [Indexed]
    public string Username { get; set; }
}
