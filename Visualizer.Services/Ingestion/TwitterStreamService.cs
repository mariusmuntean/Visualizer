using Tweetinvi;
using Tweetinvi.Streaming.V2;

namespace Visualizer.Services.Ingestion;

public class TwitterStreamService
{
    private readonly TwitterClient _twitterClient;
    private readonly TweetGraphService _tweetGraphService;
    private readonly TweetDbService _tweetDbService;
    private readonly TweetHashtagService _tweetHashtagService;
    private ISampleStreamV2? _sampleStream;

    public TwitterStreamService(TwitterClient twitterClient, TweetGraphService tweetGraphService, TweetDbService tweetDbService, TweetHashtagService tweetHashtagService)
    {
        _twitterClient = twitterClient;
        _tweetGraphService = tweetGraphService;
        _tweetDbService = tweetDbService;
        _tweetHashtagService = tweetHashtagService;
    }

    public async Task ProcessSampleStream(int amount = 10)
    {
        await _tweetGraphService.GetNodes();

        var currentAmount = 0;
        var stopped = false;

        _sampleStream = _twitterClient.StreamsV2.CreateSampleStream();
        _sampleStream.TweetReceived += async (sender, args) =>
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
                    _sampleStream.StopStream();
                    stopped = true;
                }
            }
        };

        try
        {
            await _sampleStream.StartAsync();
            Console.WriteLine("Started streaming");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void StopSampledStream()
    {
        _sampleStream?.StopStream();
        Console.WriteLine("Stopped streaming");
    }
}