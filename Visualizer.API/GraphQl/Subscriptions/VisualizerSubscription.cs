using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Visualizer.API.GraphQl.Types;
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
            Name = "rankedHashtag",
            Description = "Hashtags are published with their new rank",
            Type = typeof(HashtagTypeQl),
            Arguments = new QueryArguments(new QueryArgument<FloatGraphType>() {Name = "sampleIntervalSec", DefaultValue = 0, Description = "The sampling interval expressed in seconds."}),
            Resolver = new FuncFieldResolver<RankedHashtag>(ResolveHashtag),
            StreamResolver = new SourceStreamResolver<RankedHashtag>(GetHashtagAddedResolver)
        });

        AddField(new FieldType
        {
            Name = "rankedHashtagsChanged",
            Type = typeof(ListGraphType<HashtagTypeQl>),
            Arguments = new QueryArguments(new QueryArgument<IntGraphType> {Name = "amount", DefaultValue = 10}),
            Resolver = new FuncFieldResolver<RankedHashtag[]>(ResolveRankedHashtags),
            StreamResolver = new SourceStreamResolver<RankedHashtag[]>(GetRankedHashtagsObservable)
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

    private RankedHashtag ResolveHashtag(IResolveFieldContext context)
    {
        var message = context.Source as RankedHashtag;
        return message;
    }

    private IObservable<RankedHashtag> GetHashtagAddedResolver(IResolveFieldContext context)
    {
        var samplingIntervalSeconds = context.GetArgument<double>("sampleIntervalSec");
        return _tweetHashtagService.GetHashtagAddedObservable(samplingIntervalSeconds);
    }

    private RankedHashtag[] ResolveRankedHashtags(IResolveFieldContext context)
    {
        var message = context.Source as RankedHashtag[];
        return message!;
    }

    private IObservable<RankedHashtag[]> GetRankedHashtagsObservable(IResolveFieldContext context)
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
