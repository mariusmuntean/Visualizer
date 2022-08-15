using GraphQL.Types;
using Visualizer.API.Services.Query;

namespace Visualizer.GraphQl.Types.Tweet;

public class TweetModelsPageTypeQl : ObjectGraphType<TweetModelsPage>
{
    public TweetModelsPageTypeQl()
    {
        Field(t => t.Total, false, typeof(IntGraphType));
        Field(t => t.Tweets, true, typeof(ListGraphType<TweetTypeQl>));
    }
}
