using Visualizer.Shared.Models;

namespace Visualizer.Ingestion.Services.Services;

public interface IStreamingStatusMessagePublisher
{
    Task PublishStreamingStatus(StreamingStatusDto streamingStatusDto);
}
