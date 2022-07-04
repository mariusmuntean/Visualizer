using System.Net.Mime;
using GraphQL;
using GraphQL.Types;
using Newtonsoft.Json;
using Visualizer.GraphQl.Types;
using Visualizer.Services.Ingestion;
using Visualizer.Services.Query;

namespace Visualizer.GraphQl.Queries;

public class HashtagQuery : ObjectGraphType
{
    public HashtagQuery(IServiceProvider provider)
    {
        var tweetHashtagService = provider.CreateScope().ServiceProvider.GetService<TweetHashtagService>();

        FieldAsync<ListGraphType<HashtagTypeQl>>("TopHashtags",
            arguments: new QueryArguments(
                new QueryArgument<IntGraphType> { Name = "amount", DefaultValue = 10 }
            ),
            resolve: async context =>
            {
                var amount = context.GetArgument<int>("amount");
                var hts = await tweetHashtagService.GetTopHashtags(amount);
                return hts.ToArray();
            });
    }
}

public class TweetQuery : ObjectGraphType
{
    public TweetQuery(IServiceProvider provider)
    {
        var tweetDbQueryService = provider.CreateScope().ServiceProvider.GetService<TweetDbQueryService>();

        Field<IntGraphType>("do",
            resolve: context =>
            {
                // var f = tweetDbQueryService!.FindTweets(new FindTweetsInputDto { AuthorId = "3304122136", TweetId = "1544054420538904576" });
                var f = tweetDbQueryService!.FindTweets(new FindTweetsInputDto { SearchTerm = "Actuariat + gestion des risque" });
                Console.WriteLine(JsonConvert.SerializeObject(f));
                return 42;
            });
    }
}
