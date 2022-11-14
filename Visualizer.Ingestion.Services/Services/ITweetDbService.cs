using Tweetinvi.Events.V2;

namespace Visualizer.Ingestion.Services.Services;

public interface ITweetDbService
{
    Task Store(TweetV2EventArgs tweetV2ReceivedEventArgs);
}
