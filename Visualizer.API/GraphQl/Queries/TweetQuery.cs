using GraphQL;
using GraphQL.Types;
using Visualizer.API.GraphQl.Types.Input;
using Visualizer.API.GraphQl.Types.Tweet;
using Visualizer.API.Services.Services;
using Visualizer.API.Services.Services.Impl;

namespace Visualizer.API.GraphQl.Queries;

public class TweetQuery : ObjectGraphType
{
    public TweetQuery(IServiceProvider provider)
    {
        var tweetDbQueryService = provider.CreateScope().ServiceProvider.GetRequiredService<ITweetDbQueryService>();

        FieldAsync<TweetModelsPageTypeQl>("find",
        arguments: new QueryArguments(
            new QueryArgument<FindTweetsInputTypeQl> { Name = "filter" }
        ),
        resolve: async context =>
            {
                var filter = context.GetArgument<FindTweetsInputDto>("filter");
                var tweetModelsPage = await tweetDbQueryService.FindTweetsWithExpression(filter).ConfigureAwait(false);
                //var tweetModelsPage = await tweetDbQueryService.FindTweetsWithAggregation(filter).ConfigureAwait(false);
                return tweetModelsPage;
            });
    }
}
