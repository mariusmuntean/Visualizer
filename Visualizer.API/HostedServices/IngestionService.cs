using System.Reactive.Linq;
using System.Reactive.Subjects;
using Visualizer.API.Clients;
using Visualizer.API.Services.Ingestion;
using Visualizer.Shared.Models;

namespace Visualizer.HostedServices;

public class IngestionService : IHostedService, IIngestionService
{
    private readonly IIngestionClient _ingestionClient;
    private readonly ILogger<IngestionService> _logger;

    private ISubject<StreamingStatusDto> _streamingStatusStream;

    public IngestionService(IIngestionClient ingestionClient, ILogger<IngestionService> logger)
    {
        _ingestionClient = ingestionClient;
        _logger = logger;

        _streamingStatusStream = new ReplaySubject<StreamingStatusDto>(1);
    }

    public bool? IsStreaming { get; private set; }

    public async Task<HttpResponseMessage> StartStreaming()
    {
        return await _ingestionClient.StartStreaming();
    }

    public async Task<HttpResponseMessage> StopStreaming()
    {
        return await _ingestionClient.StopStreaming();
    }

    public IObservable<StreamingStatusDto> GetStreamingStatusObservable()
    {
        return _streamingStatusStream.AsObservable();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{ServiceName} is starting", nameof(IngestionService));

        // Subscribe to streaming status changes and publish subscription updates
        // ToDo:

        // Get current streaming status
        var (isStreamingResponse, streamingStatus) = await _ingestionClient.IsStreamingRunning();
        if (isStreamingResponse.IsSuccessStatusCode)
        {
            IsStreaming = streamingStatus.IsStreaming;
            _logger.LogInformation("{ServiceName} failed to retrieve the streaming service status", nameof(IngestionService));
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{ServiceName} is stopping", nameof(IngestionService));
        return Task.CompletedTask;
    }
}
