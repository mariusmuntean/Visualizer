using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Visualizer.GraphQl.Types;
using Visualizer.HostedServices;
using Visualizer.Services.Ingestion;

namespace Visualizer.GraphQl.Subscriptions;

public class VisualizerSubscription : ObjectGraphType
{
    private readonly TweetHashtagService _tweetHashtagService;
    private readonly TweeterStreamingStarterService _tweeterStreamingStarterService;

    public VisualizerSubscription(IServiceProvider provider)
    {
        _tweetHashtagService = provider.CreateScope().ServiceProvider.GetRequiredService<TweetHashtagService>();
        _tweeterStreamingStarterService = provider.CreateScope().ServiceProvider.GetRequiredService<TweeterStreamingStarterService>();

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
            Type = typeof(IsStreamingStateTypeQl),
            Resolver = new FuncFieldResolver<TweeterStreamingStarterService.IsStreamingState>(ResolveIsStreaming),
            StreamResolver = new SourceStreamResolver<TweeterStreamingStarterService.IsStreamingState>(SubscribeIsStreaming)
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
        return message;
    }

    private IObservable<TweetHashtagService.ScoredHashtag[]> SubscribeToRankedHashtags(IResolveFieldContext context)
    {
        var amount = context.GetArgument<int>("amount");
        return _tweetHashtagService.GetRankedHashtagsObservable(amount);
    }

    private TweeterStreamingStarterService.IsStreamingState ResolveIsStreaming(IResolveFieldContext context)
    {
        var message = context.Source;

        return message as TweeterStreamingStarterService.IsStreamingState;
    }

    private IObservable<TweeterStreamingStarterService.IsStreamingState> SubscribeIsStreaming(IResolveFieldContext context)
    {
        return _tweeterStreamingStarterService.GetIsStreamingObservable();
    }
}