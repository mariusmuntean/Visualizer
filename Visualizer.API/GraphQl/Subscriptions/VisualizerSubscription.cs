using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Visualizer.API.GraphQl.Types;
using Visualizer.API.Services.DTOs;
using Visualizer.API.Services.Services;
using Visualizer.Shared.Models;

namespace Visualizer.API.GraphQl.Subscriptions;

public class VisualizerSubscription : ObjectGraphType
{
    private readonly ITweetHashtagService _tweetHashtagService;
    private readonly IIngestionServiceProxy _ingestionService;

    public VisualizerSubscription(IServiceProvider provider)
    {
        _tweetHashtagService = provider.CreateScope().ServiceProvider.GetRequiredService<ITweetHashtagService>();
        _ingestionService = provider.CreateScope().ServiceProvider.GetRequiredService<IIngestionServiceProxy>();

        AddField(new FieldType
        {
            Name = "hashtagAdded",
            Type = typeof(HashtagTypeQl),
            Resolver = new FuncFieldResolver<ScoredHashtag>(ResolveHashtag),
            StreamResolver = new SourceStreamResolver<ScoredHashtag>(GetHashtagAddedResolver)
        });

        AddField(new FieldType
        {
            Name = "rankedHashtagsChanged",
            Type = typeof(ListGraphType<HashtagTypeQl>),
            Arguments = new QueryArguments(new QueryArgument<IntGraphType> {Name = "amount", DefaultValue = 10}),
            Resolver = new FuncFieldResolver<ScoredHashtag[]>(ResolveRankedHashtags),
            StreamResolver = new SourceStreamResolver<ScoredHashtag[]>(GetRankedHashtagsObservable)
        });

        AddField(new FieldType
        {
            Name = "isStreamingChanged",
            Description = "Produces updates whenever the state of the live ingestion has changed",
            Type = typeof(StreamingStatusTypeQl),
            Resolver = new FuncFieldResolver<StreamingStatusDto>(ResolveIsStreaming),
            StreamResolver = new SourceStreamResolver<StreamingStatusDto>(GetStreamingStatusObservable)
        });
    }

    private ScoredHashtag ResolveHashtag(IResolveFieldContext context)
    {
        var message = context.Source as ScoredHashtag;
        return message;
    }

    private IObservable<ScoredHashtag> GetHashtagAddedResolver(IResolveFieldContext context)
    {
        return _tweetHashtagService.GetHashtagAddedObservable();
    }

    private ScoredHashtag[] ResolveRankedHashtags(IResolveFieldContext context)
    {
        var message = context.Source as ScoredHashtag[];
        return message!;
    }

    private IObservable<ScoredHashtag[]> GetRankedHashtagsObservable(IResolveFieldContext context)
    {
        var amount = context.GetArgument<int>("amount");
        return _tweetHashtagService.GetRankedHashtagsObservable(amount);
    }

    private StreamingStatusDto ResolveIsStreaming(IResolveFieldContext context)
    {
        var message = context.Source as StreamingStatusDto;
        return message!;
    }

    private IObservable<StreamingStatusDto> GetStreamingStatusObservable(IResolveFieldContext context) => _ingestionService.GetStreamingStatusObservable();
}
