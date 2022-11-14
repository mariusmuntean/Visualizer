using Tweetinvi.Events.V2;

namespace Visualizer.Ingestion.Services.Services;

public interface ITweetHashtagService
{
    Task AddHashtags(TweetV2EventArgs tweetV2ReceivedEventArgs);
    Task AddHashtag(string hashtag);
}
