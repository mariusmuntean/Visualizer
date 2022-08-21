using GraphQL.Types;
using Visualizer.Shared.Models;

namespace Visualizer.API.GraphQl.Types.Graph;

public class UserNodeTypeQl : ObjectGraphType<UserNode>
{
    public UserNodeTypeQl()
    {
        Field(node => node.UserId, false);
        Field(node => node.UserName, true);
    }
}