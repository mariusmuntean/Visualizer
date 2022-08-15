// using Mapster;
// using Microsoft.Extensions.Logging;
// using Redis.OM;
// using Tweetinvi.Events.V2;
// using Visualizer.API.Model.TweetDb;
//
// namespace Visualizer.API.Services.Ingestion;
//
// public class TweetDbService
// {
//     private readonly RedisConnectionProvider _redisConnectionProvider;
//     private readonly ILogger<TweetDbService> _logger;
//
//     public TweetDbService(RedisConnectionProvider redisConnectionProvider, ILogger<TweetDbService> logger)
//     {
//         _redisConnectionProvider = redisConnectionProvider;
//         _logger = logger;
//     }
//
//     public async Task Store(TweetV2ReceivedEventArgs tweetV2ReceivedEventArgs)
//     {
//         string internalId;
//         try
//         {
//             var tweetCollection = _redisConnectionProvider.RedisCollection<TweetModel>();
//             var tweetModel = tweetV2ReceivedEventArgs.Adapt<TweetModel>();
//             internalId = await tweetCollection.InsertAsync(tweetModel);
//         }
//         catch (Exception e)
//         {
//             _logger.LogError(e, "Failed to process tweet event {Id}", tweetV2ReceivedEventArgs?.Tweet?.Id);
//         }
//     }
// }
