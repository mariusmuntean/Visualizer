using System;
using System.Linq;
using Mapster;
using Redis.OM.Modeling;
using Tweetinvi.Events.V2;
using Tweetinvi.Models.V2;

namespace Visualizer.Shared.Models;

public class VisualizerMapster
{
    public static void Configure()
    {
        // Config Mapster
        TypeAdapterConfig<DateTimeOffset, DateTime>.NewConfig()
            .MapWith(offset => offset.DateTime)
            ;
        TypeAdapterConfig<TweetV2ReceivedEventArgs, TweetModel>.NewConfig()
            .Map(dest => dest.Id, src => src.Tweet.Id)
            .Map(dest => dest.AuthorId, src => src.Tweet.AuthorId)
            .Map(dest => dest.Text, src => src.Tweet.Text)
            .Map(dest => dest.CreatedAt, src => src.Tweet.CreatedAt.UtcTicks)
            .Map(dest => dest.ConversationId, src => src.Tweet.ConversationId)
            .Map(dest => dest.Username, src => src.Includes.Users.FirstOrDefault(u => u.Id == src.Tweet.AuthorId).Username)
            .Map(dest => dest.Entities, src => src.Tweet.Entities.Adapt<TweetEntities>())
            .Map(dest => dest.Lang, src => src.Tweet.Lang)
            .Map(dest => dest.Source, src => src.Tweet.Source)
            .Map(dest => dest.OrganicMetrics, src => src.Tweet.OrganicMetrics)
            .Map(dest => dest.PublicMetricsLikeCount, src => src.Tweet.PublicMetrics.LikeCount)
            .Map(dest => dest.PublicMetricsQuoteCount, src => src.Tweet.PublicMetrics.QuoteCount)
            .Map(dest => dest.PublicMetricsReplyCount, src => src.Tweet.PublicMetrics.ReplyCount)
            .Map(dest => dest.PublicMetricsRetweetCount, src => src.Tweet.PublicMetrics.RetweetCount)
            .Map(dest => dest.ReferencedTweets, src => src.Tweet.ReferencedTweets)
            .Map(dest => dest.GeoLoc,
                src => new GeoLoc(src.Tweet.Geo.Coordinates.Coordinates[0], src.Tweet.Geo.Coordinates.Coordinates[1]),
                src => src.Tweet.Geo != null && src.Tweet.Geo.Coordinates != null && src.Tweet.Geo.Coordinates.Coordinates != null && src.Tweet.Geo.Coordinates.Coordinates != null && src.Tweet.Geo.Coordinates.Coordinates.Length > 0)
            ;
        TypeAdapterConfig<TweetEntitiesV2, TweetEntities>.NewConfig()
            .Map(dest => dest.Hashtags, src => src.Hashtags.Select(h => h.Tag), src => src.Hashtags != null)
            .Map(dest => dest.Cashtags, src => src.Cashtags.Select(c => c.Tag), src => src.Cashtags != null)
            .Map(dest => dest.Mentions, src => src.Mentions.Select(m => m.Username), src => src.Mentions != null)
            ;
    }
}
