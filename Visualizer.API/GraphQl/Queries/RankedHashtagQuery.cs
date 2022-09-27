using GraphQL;
using GraphQL.Types;
using Visualizer.API.GraphQl.Types;
using Visualizer.API.Services.Services;

namespace Visualizer.API.GraphQl.Queries;

// ReSharper disable once ClassNeverInstantiated.Global
public class RankedHashtagQuery : ObjectGraphType
{
    public RankedHashtagQuery(IServiceProvider provider)
    {
        var tweetHashtagService = provider.CreateScope().ServiceProvider.GetService<ITweetHashtagService>();

        FieldAsync<ListGraphType<RankedHashtagTypeQl>>("topRankedHashtags",
            description: "Retrieve a specified amount of the top ranked hashtags",
            arguments: new QueryArguments(
                new QueryArgument<IntGraphType> {Name = "amount", DefaultValue = 10}
            ),
            resolve: async context =>
            {
                var amount = context.GetArgument<int>("amount");
                var hts = await tweetHashtagService.GetTopHashtags(amount);
                return hts.ToArray();
            });
    }
}
