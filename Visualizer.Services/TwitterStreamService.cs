using Tweetinvi;

namespace Visualizer.Services;

public class TwitterStreamService
{
    private readonly TwitterClient _twitterClient;
    private readonly TweetGraphService _tweetGraphService;
    private readonly TweetDbService _tweetDbService;
    private readonly TweetHashtagService _tweetHashtagService;

    public TwitterStreamService(TwitterClient twitterClient, TweetGraphService tweetGraphService, TweetDbService tweetDbService, TweetHashtagService tweetHashtagService)
    {
        _twitterClient = twitterClient;
        _tweetGraphService = tweetGraphService;
        _tweetDbService = tweetDbService;
        _tweetHashtagService = tweetHashtagService;
    }

    public async Task ProcessSampleStream(int amount = 10)
    {
        var currentAmount = 0;
        var stopped = false;

        var filteredStream = _twitterClient.StreamsV2.CreateSampleStream();
        filteredStream.TweetReceived += async (sender, args) =>
        {
            try
            {
                currentAmount++;
                await Task.WhenAll(
                    _tweetHashtagService.AddHashtags(args),
                    _tweetGraphService.AddNodes(args),
                    _tweetDbService.Store(args)
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                if (currentAmount >= amount && !stopped)
                {
                    filteredStream.StopStream();
                    stopped = true;
                }
            }
        };

        await filteredStream.StartAsync();
    }
}