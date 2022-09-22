using Redis.OM.Modeling;

namespace Visualizer.API.Model.TweetDb;

public class CashtagEntity
{
    /// <summary>
    /// Index of the first letter of the cashtag
    /// </summary>
    [Indexed]
    public int Start { get; set; }

    /// <summary>
    /// Index of the last letter of the cashtag
    /// </summary>
    [Indexed]
    public int End { get; set; }

    /// <summary>
    /// The text of the Cashtag.
    /// </summary>
    [Indexed]
    public string Tag { get; set; }

    [Indexed]
    private string Cashtag
    {
        get => Tag;
        set => Tag = value;
    }
}
