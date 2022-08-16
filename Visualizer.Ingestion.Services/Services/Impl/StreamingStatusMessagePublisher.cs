using System.Globalization;
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
    }

    public async Task PublishStreamingStatus(StreamingStatusDto streamingStatusDto)
    {
        var streamingStatusStr = streamingStatusDto.IsStreaming.ToString(CultureInfo.InvariantCulture);
        var _ = await _subscriber.PublishAsync(new RedisChannel(StreamingConstants.StreamingStatusChannel, RedisChannel.PatternMode.Literal), new RedisValue(streamingStatusStr)).ConfigureAwait(false);
    }
}
