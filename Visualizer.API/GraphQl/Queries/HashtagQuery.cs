using GraphQL;
using GraphQL.Types;
using Visualizer.API.GraphQl.Types;
using Visualizer.API.Services.Ingestion;

namespace Visualizer.API.GraphQl.Queries;

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
