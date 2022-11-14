using Newtonsoft.Json;
using StackExchange.Redis;
using Visualizer.Shared.Constants;
using Visualizer.Shared.Models;

namespace Visualizer.Ingestion.Services.Services.Impl;

internal class HashtagRankedMessagePublisher : IHashtagRankedMessagePublisher
{
    private readonly ISubscriber _subscriber;

    public HashtagRankedMessagePublisher(ISubscriber subscriber)
    {
        _subscriber = subscriber;
        // ToDo: use a private System.Threading.Channel to decouple receiving and publishing messages
    }

    public async Task PublishRankedHashtagMessage(string hashtag, int rank)
    {
        var channel = new RedisChannel(HashtagConstants.RankedHashtagChannelName, RedisChannel.PatternMode.Literal);
        var rankedHashtagStr = JsonConvert.SerializeObject(new RankedHashtag() {Name = hashtag, Rank = rank});
        var _ = await _subscriber.PublishAsync(channel, new RedisValue(rankedHashtagStr)).ConfigureAwait(false);
    }
}
