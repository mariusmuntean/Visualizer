using Tweetinvi.Events.V2;

namespace Visualizer.Ingestion.Services.Services;

public interface ITweetGraphService
{
    Task AddNodes(TweetV2EventArgs tweetV2ReceivedEventArgs);
}
