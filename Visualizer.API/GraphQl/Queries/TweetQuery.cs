using GraphQL;
using GraphQL.Types;
using Newtonsoft.Json;
using Visualizer.API.GraphQl.Types.Input;
using Visualizer.API.GraphQl.Types.Tweet;
using Visualizer.API.Services.Query;

namespace Visualizer.API.GraphQl.Queries;

public class TweetQuery : ObjectGraphType
{
    public TweetQuery(IServiceProvider provider)
    {
        var tweetDbQueryService = provider.CreateScope().ServiceProvider.GetService<TweetDbQueryService>();

        FieldAsync<TweetModelsPageTypeQl>("find",
        arguments: new QueryArguments(
            new QueryArgument<FindTweetsInputTypeQl> { Name = "filter" }
        ),
        resolve: async context =>
            {
                var filter = context.GetArgument<FindTweetsInputDto>("filter");
                var tweetModelsPage = await tweetDbQueryService!.FindTweetsWithExpression(filter);
                Console.WriteLine(JsonConvert.SerializeObject(tweetModelsPage));
                return tweetModelsPage;
            });
    }
}
