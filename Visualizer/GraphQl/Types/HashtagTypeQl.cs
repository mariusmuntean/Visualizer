using GraphQL.Types;
using Visualizer.HostedServices;
using Visualizer.Services.Ingestion;

namespace Visualizer.GraphQl.Types;

public class HashtagTypeQl : ObjectGraphType<TweetHashtagService.ScoredHashtag>
{
    public HashtagTypeQl()
    {
        Field(entry => entry.Name, false, typeof(StringGraphType));
        Field(entry => entry.Score, false, typeof(DecimalGraphType));

        // ToDo: add another typeQl class for GraphResult
    }
}

public class IsStreamingStateTypeQl : ObjectGraphType<TweeterStreamingStarterService.IsStreamingState>
{
    public IsStreamingStateTypeQl()
    {
        Field(entry => entry.IsStreaming, false, typeof(BooleanGraphType));
    }
}