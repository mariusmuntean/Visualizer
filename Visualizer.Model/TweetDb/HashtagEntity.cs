
using Redis.OM.Modeling;

namespace Visualizer.Model.TweetDb;

public class HashtagEntity
{
    /// <summary>
    /// Index of the first letter of the tag
    /// </summary>
    [Indexed]
    public int Start { get; set; }

    /// <summary>
    /// Index of the last letter of the tag
    /// </summary>
    [Indexed]
    public int End { get; set; }

    /// <summary>
    /// The text of the Hashtag.
    /// </summary>
    [Indexed]
    public string Tag { get; set; }

    [Indexed]
    public string Hashtag
    {
        set => Tag = value;
    }
}
