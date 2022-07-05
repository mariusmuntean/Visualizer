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

        Field<ListGraphType<TweetTypeQl>>("find",
        arguments: new QueryArguments(
            new QueryArgument<FindTweetsInputTypeQl>() { Name = "filter" }
        ),
        resolve: context =>
            {
                var filter = context.GetArgument<FindTweetsInputDto>("filter");
                var tweets = tweetDbQueryService!.FindTweets(filter);
                Console.WriteLine(JsonConvert.SerializeObject(tweets));
                return tweets;
            });
    }
}
