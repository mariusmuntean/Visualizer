using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Visualizer.API.Clients;
using Visualizer.Shared.Constants;
using Visualizer.Shared.Models;

namespace Visualizer.API.Services.Services.Impl;

/// <summary>
/// A proxy service for the Ingestion microservice. It allows to control the streaming.
/// This service is also a IHostedService. On system startup if subscribes to the Ingestion microservice to receive streaming status changes.
/// </summary>
internal class IngestionServiceProxy : IIngestionServiceProxy
{
    private readonly IIngestionClient _ingestionClient;
    private readonly ISubscriber _subscriber;
    private readonly ILogger<IngestionServiceProxy> _logger;

    private readonly ISubject<StreamingStatusDto> _streamingStatusStream;

    public IngestionServiceProxy(IIngestionClient ingestionClient, ISubscriber subscriber, ILogger<IngestionServiceProxy> logger)
    {
        _ingestionClient = ingestionClient;
        _subscriber = subscriber;
        _logger = logger;

        _streamingStatusStream = new ReplaySubject<StreamingStatusDto>(1);
    }

    public bool? IsStreaming { get; private set; }

    public async Task<HttpResponseMessage> StartStreaming()
    {
        return await _ingestionClient.StartStreaming().ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> StopStreaming()
    {
        return await _ingestionClient.StopStreaming().ConfigureAwait(false);
    }

    public IObservable<StreamingStatusDto> GetStreamingStatusObservable()
    {
        return _streamingStatusStream.AsObservable();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{ServiceName} is starting", nameof(IngestionServiceProxy));

        // Subscribe to streaming status changes and publish subscription updates
        var channelMessageQueue = await _subscriber.SubscribeAsync(new RedisChannel(StreamingConstants.StreamingStatusChannel, RedisChannel.PatternMode.Literal)).ConfigureAwait(false);
        channelMessageQueue.OnMessage(message =>
        {
            if (!message.Message.HasValue || message.Message.IsNullOrEmpty)
            {
                _logger.LogWarning("Ingestion service status change message was null er empty");
                return;
            }

            var isStreaming = bool.Parse(Encoding.UTF8.GetString(message.Message));
            _logger.LogInformation("Ingestion service status changed to: {IsStreaming}", isStreaming);

            IsStreaming = isStreaming;
            PublishCurrentStreamingStatus(isStreaming);
        });


        // Get current streaming status
        var (isStreamingResponse, streamingStatus) = await _ingestionClient.IsStreamingRunning().ConfigureAwait(false);
        if (isStreamingResponse.IsSuccessStatusCode)
        {
            IsStreaming ??= streamingStatus.IsStreaming; // assign the new value only if IsStreaming has no value. IsStreaming may already have a value if one was set via PubSub.
            _logger.LogInformation("{ServiceName} failed to retrieve the streaming service status", nameof(IngestionServiceProxy));
        }

        _logger.LogInformation("{ServiceName} started", nameof(IngestionServiceProxy));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{ServiceName} is stopping", nameof(IngestionServiceProxy));
        return Task.CompletedTask;
    }

    private void PublishCurrentStreamingStatus(bool isStreaming)
    {
        _streamingStatusStream.OnNext(new StreamingStatusDto {IsStreaming = isStreaming});
    }
}
