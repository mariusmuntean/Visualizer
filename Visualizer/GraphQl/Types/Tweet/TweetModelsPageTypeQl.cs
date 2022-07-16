using GraphQL.Types;
using Visualizer.Services.Query;

namespace Visualizer.GraphQl.Types.Tweet;

public class TweetModelsPageTypeQl : ObjectGraphType<TweetModelsPage>
{
    public TweetModelsPageTypeQl()
    {
        Field(t => t.Total, false, typeof(IntGraphType));
        Field(t => t.Tweets, true, typeof(ListGraphType<TweetTypeQl>));
    }
}
