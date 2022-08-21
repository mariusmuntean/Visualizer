using GraphQL;
using GraphQL.Types;
using Visualizer.API.GraphQl.Types;
using Visualizer.API.GraphQl.Types.Graph;
using Visualizer.API.GraphQl.Types.Input;
using Visualizer.API.Services.DTOs;
using Visualizer.API.Services.Services;

namespace Visualizer.API.GraphQl.Queries;

public class GraphResultQuery : ObjectGraphType
{
    public GraphResultQuery(IServiceProvider provider)
    {
        var tweetGraphService = provider.CreateScope().ServiceProvider.GetService<ITweetGraphService>();

        FieldAsync<GraphResultTypeQl>("graphResults",
            arguments: new QueryArguments(
                new QueryArgument<IntGraphType> { Name = "amount", DefaultValue = 10 }
            ),
            resolve: async context =>
            {
                var amount = context.GetArgument<int>("amount");
                var graphResult = await tweetGraphService.GetNodes(amount);
                return graphResult;
            });

        FieldAsync<GraphResultTypeQl>("mentions",
            arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<MentionFilterInputTypeQl>>
                {
                    Name = "filter",
                    DefaultValue = new MentionFilterDto { Amount = 400, AuthorUserName = null, MentionedUserNames = null, MinHops = 1, MaxHops = 10 }
                }
            ),
            resolve: async context =>
            {
                var mentionFilterDto = context.GetArgument<MentionFilterDto>("filter");
                var graphResult = await tweetGraphService.GetMentions(mentionFilterDto);
                return graphResult;
            });

        FieldAsync<LongGraphType>("userCount",
            resolve: async context =>
            {
                var userCount = await tweetGraphService.CountUsers();
                return userCount;
            });
    }
}
