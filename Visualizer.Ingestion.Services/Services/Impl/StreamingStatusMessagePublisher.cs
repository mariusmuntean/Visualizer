using System.Globalization;
using System.Text.Json.Serialization;
using StackExchange.Redis;
using Visualizer.Shared.Constants;
using Visualizer.Shared.Models;

namespace Visualizer.Ingestion.Services.Services.Impl;

internal class StreamingStatusMessagePublisher : IStreamingStatusMessagePublisher
{
    private readonly ISubscriber _subscriber;

    public StreamingStatusMessagePublisher(ISubscriber subscriber)
    {
        _subscriber = subscriber;

        // ToDo: use a private System.Threading.Channel to decouple receiving and publishing messages
    }

    public async Task PublishStreamingStatus(StreamingStatusDto streamingStatusDto)
    {
        var streamingStatusStr = streamingStatusDto.IsStreaming.ToString(CultureInfo.InvariantCulture);
        var _ = await _subscriber.PublishAsync(new RedisChannel(StreamingConstants.StreamingStatusChannel, RedisChannel.PatternMode.Literal), new RedisValue(streamingStatusStr)).ConfigureAwait(false);
    }
}
