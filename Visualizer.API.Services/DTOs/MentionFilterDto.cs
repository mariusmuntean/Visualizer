namespace Visualizer.API.Services.DTOs;

public class MentionFilterDto
{
    public string? AuthorUserName { get; set; }
    public string[]? MentionedUserNames { get; set; }
    public int Amount { get; set; }

    public int MinHops { get; set; } = 1;
    public int MaxHops { get; set; } = 10;

    public void Deconstruct(out string? authorUserName, out string[]? mentionedUserNames, out int amount, out int minHops, out int maxHops)
    {
        authorUserName = AuthorUserName;
        mentionedUserNames = MentionedUserNames;
        amount = Amount;
        minHops = MinHops;
        maxHops = MaxHops;
    }
}