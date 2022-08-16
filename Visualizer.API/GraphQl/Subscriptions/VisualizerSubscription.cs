using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Visualizer.API.GraphQl.Types;
using Visualizer.API.HostedServices;
using Visualizer.API.Services.Ingestion;
using Visualizer.Shared.Models;

namespace Visualizer.API.GraphQl.Subscriptions;

public class VisualizerSubscription : ObjectGraphType
{
    private readonly TweetHashtagService _tweetHashtagService;
    private readonly IngestionService _ingestionService;

    public VisualizerSubscription(IServiceProvider provider)
    {
        _tweetHashtagService = provider.CreateScope().ServiceProvider.GetRequiredService<TweetHashtagService>();
        _ingestionService = provider.CreateScope().ServiceProvider.GetRequiredService<IngestionService>();

        AddField(new FieldType
        {
            Name = "hashtagAdded",
            Type = typeof(HashtagTypeQl),
            Resolver = new FuncFieldResolver<TweetHashtagService.ScoredHashtag>(ResolveHashtag),
            StreamResolver = new SourceStreamResolver<TweetHashtagService.ScoredHashtag>(Subscribe)
        });

        AddField(new FieldType
        {
            Name = "rankedHashtagsChanged",
            Type = typeof(ListGraphType<HashtagTypeQl>),
            Arguments = new QueryArguments(new QueryArgument<IntGraphType> {Name = "amount", DefaultValue = 10}),
            Resolver = new FuncFieldResolver<TweetHashtagService.ScoredHashtag[]>(ResolveRankedHashtags),
            StreamResolver = new SourceStreamResolver<TweetHashtagService.ScoredHashtag[]>(SubscribeToRankedHashtags)
        });

        AddField(new FieldType
        {
            Name = "isStreamingChanged",
            Description = "Produces updates whenever the state of the live ingestion has changed",
            Type = typeof(StreamingStatusTypeQl),
            Resolver = new FuncFieldResolver<StreamingStatusDto>(ResolveIsStreaming),
            StreamResolver = new SourceStreamResolver<StreamingStatusDto>(SubscribeIsStreaming)
        });
    }

    private TweetHashtagService.ScoredHashtag ResolveHashtag(IResolveFieldContext context)
    {
        var message = context.Source as TweetHashtagService.ScoredHashtag;

        return message;
    }

    private IObservable<TweetHashtagService.ScoredHashtag> Subscribe(IResolveFieldContext context)
    {
        return _tweetHashtagService.GetHashtagAddedObservable();
    }

    private TweetHashtagService.ScoredHashtag[] ResolveRankedHashtags(IResolveFieldContext context)
    {
        var message = context.Source as TweetHashtagService.ScoredHashtag[];
        return message!;
    }

    private IObservable<TweetHashtagService.ScoredHashtag[]> SubscribeToRankedHashtags(IResolveFieldContext context)
    {
        var amount = context.GetArgument<int>("amount");
        return _tweetHashtagService.GetRankedHashtagsObservable(amount);
    }

    private StreamingStatusDto ResolveIsStreaming(IResolveFieldContext context)
    {
        var message = context.Source as StreamingStatusDto;
        return message!;
    }

    private IObservable<StreamingStatusDto> SubscribeIsStreaming(IResolveFieldContext context) => _ingestionService.GetStreamingStatusObservable();
}
