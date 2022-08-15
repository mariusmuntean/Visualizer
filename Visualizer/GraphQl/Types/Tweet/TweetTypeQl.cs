using GraphQL.Types;
using Visualizer.Model.TweetDb;
using Visualizer.Shared.Models;

namespace Visualizer.GraphQl.Types.Tweet;

public class TweetTypeQl : ObjectGraphType<TweetModel>
{
    public TweetTypeQl()
    {
        Field(t => t.Id, false, typeof(StringGraphType));
        Field(t => t.ConversationId, false, typeof(StringGraphType));
        Field(t => t.AuthorId, false, typeof(StringGraphType));
        Field(t => t.Username, false, typeof(StringGraphType));
        Field(typeof(DateTimeGraphType), nameof(TweetModel.CreatedAt), resolve: context => new DateTime(context.Source.CreatedAt, DateTimeKind.Utc));
        Field(t => t.Lang, false, typeof(StringGraphType));
        Field(t => t.Source, false, typeof(StringGraphType));
        Field(t => t.Text, false, typeof(StringGraphType));

        Field(t => t.ReferencedTweets, true, typeof(ListGraphType<ReferencedTweetTypeQl>));
        Field(t => t.OrganicMetrics, true, typeof(TweetMetricsTypeQl));
        Field(t => t.GeoLoc, true, typeof(GeoLocTypeQl));
        Field(t => t.Entities, true, typeof(TweetEntitiesTypeQl));
    }
}
