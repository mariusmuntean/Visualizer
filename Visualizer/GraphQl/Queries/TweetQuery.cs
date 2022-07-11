using GraphQL;
using GraphQL.Types;
using Newtonsoft.Json;
using Visualizer.GraphQl.Types.Tweet;
using Visualizer.Services.Query;

namespace Visualizer.GraphQl.Queries;

public class TweetQuery : ObjectGraphType
{
    public TweetQuery(IServiceProvider provider)
    {
        var tweetDbQueryService = provider.CreateScope().ServiceProvider.GetService<TweetDbQueryService>();

        FieldAsync<ListGraphType<TweetTypeQl>>("find",
        arguments: new QueryArguments(
            new QueryArgument<FindTweetsInputTypeQl>() { Name = "filter" }
        ),
        resolve: async context =>
            {
                var filter = context.GetArgument<FindTweetsInputDto>("filter");
                var tweets = await tweetDbQueryService!.FindTweetsWithExpression(filter);
                Console.WriteLine(JsonConvert.SerializeObject(tweets));
                return tweets;
            });
    }
}
