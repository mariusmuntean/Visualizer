using Tweetinvi.Events.V2;

namespace Visualizer.Ingestion.Services.Services;

public interface ITweetHashtagService
{
    Task AddHashtags(TweetV2ReceivedEventArgs tweetV2ReceivedEventArgs);
    Task AddHashtag(string hashtag);
}
