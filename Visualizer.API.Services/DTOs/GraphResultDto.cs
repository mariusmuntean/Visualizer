using Visualizer.Shared.Models;

namespace Visualizer.API.Services.DTOs;

public class GraphResultDto
{
    public Dictionary<string, UserNode> Nodes { get; set; } = new Dictionary<string, UserNode>();

    public HashSet<MentionRelationship> Edges { get; set; } = new HashSet<MentionRelationship>();

    public GraphResultStatisticsDto? Statistics { get; set; }
}
