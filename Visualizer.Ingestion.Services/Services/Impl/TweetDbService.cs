using Mapster;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redis.OM;
using Redis.OM.Searching;
using Tweetinvi.Events.V2;
using Visualizer.Shared.Models;

namespace Visualizer.Ingestion.Services.Services.Impl;

internal class TweetDbService : ITweetDbService
{
    private readonly RedisConnectionProvider _redisConnectionProvider;
    private readonly ILogger<TweetDbService> _logger;
    private IRedisCollection<TweetModel> _tweetCollection;

    public TweetDbService(RedisConnectionProvider redisConnectionProvider, ILogger<TweetDbService> logger)
    {
        _redisConnectionProvider = redisConnectionProvider;
        _logger = logger;

        _tweetCollection = _redisConnectionProvider.RedisCollection<TweetModel>();
    }

    public async Task Store(TweetV2EventArgs tweetV2ReceivedEventArgs)
    {
        string internalId;
        try
        {

            var tweetModel = tweetV2ReceivedEventArgs.Adapt<TweetModel>();
            internalId = await _tweetCollection.InsertAsync(tweetModel);
            if (tweetModel.HasGeoLoc == "1")
            {
                Console.WriteLine($"Geo tweet {JsonConvert.SerializeObject(tweetModel)}");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to process tweet event {Id}", tweetV2ReceivedEventArgs?.Tweet?.Id);
        }
    }
}
