using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Visualizer.GraphQl.Types;
using Visualizer.Services;

namespace Visualizer.GraphQl.Subscriptions;

public class HashtagSubscription : ObjectGraphType
{
    private readonly TweetHashtagService _tweetHashtagService;

    public HashtagSubscription(IServiceProvider provider)
    {
        _tweetHashtagService = provider.CreateScope().ServiceProvider.GetRequiredService<TweetHashtagService>();

        AddField(new FieldType
        {
            Name = "hashtagAdded",
            Type = typeof(HashtagTypeQl),
            Resolver = new FuncFieldResolver<TweetHashtagService.ScoredHashtag>(ResolveHashtag),
            StreamResolver = new SourceStreamResolver<TweetHashtagService.ScoredHashtag>(Subscribe)
        });
    }

    private TweetHashtagService.ScoredHashtag ResolveHashtag(IResolveFieldContext context)
    {
        var message = context.Source as TweetHashtagService.ScoredHashtag;

        return message;
    }

    private IObservable<TweetHashtagService.ScoredHashtag> Subscribe(IResolveFieldContext context)
    {
        return _tweetHashtagService.Hashtags();
    }
}