using GraphQL;
using GraphQL.Types;
using Visualizer.GraphQl.Types;
using Visualizer.Services.Ingestion;

namespace Visualizer.GraphQl.Queries;

public class GraphResultQuery : ObjectGraphType
{
    public GraphResultQuery(IServiceProvider provider)
    {
        var tweetGraphService = provider.CreateScope().ServiceProvider.GetService<TweetGraphService>();

        FieldAsync<GraphResultTypeQl>("GraphResults",
            arguments: new QueryArguments(
                new QueryArgument<IntGraphType> {Name = "amount", DefaultValue = 10}
            ),
            resolve: async context =>
            {
                var amount = context.GetArgument<int>("amount");
                var graphResult = await tweetGraphService.GetNodes(amount);
                return graphResult;
            });
    }
}