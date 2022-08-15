// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using StackExchange.Redis;
using Visualizer.Shared.Constants;
using Visualizer.Shared.Models;

namespace Visualizer.Ingestion.Services;

public class StreamingStatusMessagePublisher : IStreamingStatusMessagePublisher
{
    private readonly ISubscriber _subscriber;
    private readonly TwitterStreamService _twitterStreamService;

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
