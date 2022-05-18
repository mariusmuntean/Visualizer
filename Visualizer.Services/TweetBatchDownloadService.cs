using Redis.OM;
using Tweetinvi;
using Visualizer.Model;

namespace Visualizer.Services;

public class TweetBatchDownloadService
{
    private readonly TwitterClient _twitterClient;
    private readonly RedisConnectionProvider _redisConnectionProvider;

    public TweetBatchDownloadService(TwitterClient twitterClient, RedisConnectionProvider redisConnectionProvider)
    {
        _twitterClient = twitterClient;
        _redisConnectionProvider = redisConnectionProvider;
    }

    public async Task BatchDownloadTweets(int amount = 10)
    {
        var currentAmount = 0;

        var tweetCollection = _redisConnectionProvider.RedisCollection<TweetModel>();

        var sampleStreamV2 = _twitterClient.StreamsV2.CreateSampleStream();
        sampleStreamV2.TweetReceived += async (sender, args) =>
        {
            try
            {
                currentAmount++;
                Console.WriteLine(args.Tweet.Text);
                var internalId = await tweetCollection.InsertAsync(new TweetModel()
                {
                    Id = args.Tweet.Id,
                    AuthorId = args.Tweet.AuthorId,
                    CreatedAt = args.Tweet.CreatedAt.DateTime,
                    Text = args.Tweet.Text
                });
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
                    sampleStreamV2.StopStream();
                }
            }
        };

        await sampleStreamV2.StartAsync();
    }
}