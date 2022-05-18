using Mapster;
using Redis.OM;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters.V2;
using Visualizer.Model;

namespace Visualizer.Services;

public class TweetBatchDownloadService
{
    private readonly TwitterClient _twitterClient;
    private readonly RedisConnectionProvider _redisConnectionProvider;
    private readonly TweetGraphService _tweetGraphService;

    public TweetBatchDownloadService(TwitterClient twitterClient, RedisConnectionProvider redisConnectionProvider, TweetGraphService tweetGraphService)
    {
        _twitterClient = twitterClient;
        _redisConnectionProvider = redisConnectionProvider;
        _tweetGraphService = tweetGraphService;
    }

    public async Task BatchDownloadTweets(int amount = 10)
    {
        var currentAmount = 0;

        var tweetCollection = _redisConnectionProvider.RedisCollection<TweetModel>();

        var filteredStream = _twitterClient.StreamsV2.CreateSampleStream();
        filteredStream.TweetReceived += async (sender, args) =>
        {
            try
            {
                currentAmount++;
                var tweetModel = args.Tweet.Adapt<TweetModel>();
                var internalId = await tweetCollection.InsertAsync(tweetModel);
                await _tweetGraphService.AddTweet(args);
                Console.WriteLine(internalId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                if (currentAmount >= amount)
                {
                    filteredStream.StopStream();
                }
            }
        };

        await filteredStream.StartAsync();
    }
}