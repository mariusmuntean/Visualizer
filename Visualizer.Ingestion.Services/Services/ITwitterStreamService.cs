namespace Visualizer.Ingestion.Services.Services;

public interface ITwitterStreamService
{
    bool IsStreaming { get; }
    Task ProcessSampleStream();
    Task StopSampledStream();
}
