using GraphQL.Types;
using Visualizer.Services;

namespace Visualizer.GraphQl.Types;

public class GraphResultTypeQl : ObjectGraphType<TweetGraphService.GraphResult>
{
    public GraphResultTypeQl()
    {
        Field(nameof(TweetGraphService.GraphResult.Nodes),
            result => result.Nodes.Select(pair => new GraphNode { Id = pair.Key, Label = "unknown" }),
            false, typeof(ListGraphType<GraphNodeTypeQl>));

        Field(nameof(TweetGraphService.GraphResult.Edges),
            result => result.Edges.Select(pair => new GraphEdge() { ToId = pair.fromId, FromId = pair.toId }),
            false, typeof(ListGraphType<GraphEdgeTypeQl>));
    }
}

public class GraphNode
{
    public string Id { get; set; }
    public string Label { get; set; }
    public string Title { get; set; }
}

public class GraphNodeTypeQl : ObjectGraphType<GraphNode>
{
    public GraphNodeTypeQl()
    {
        Field(node => node.Id, false);
        Field(node => node.Label, true);
        Field(node => node.Title, true);
    }
}

public class GraphEdge
{
    public string FromId { get; set; }
    public string ToId { get; set; }
}

public class GraphEdgeTypeQl : ObjectGraphType<GraphEdge>
{
    public GraphEdgeTypeQl()
    {
        Field(graphEdge => graphEdge.FromId, false);
        Field(graphEdge => graphEdge.ToId, false);
    }
}