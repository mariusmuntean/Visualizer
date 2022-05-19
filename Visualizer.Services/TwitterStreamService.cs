using Tweetinvi;
using Tweetinvi.Parameters;
using Tweetinvi.Parameters.V2;

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

    public async Task ProcessFilteredStream(int amount = 10)
    {
        // Delete previous rules
        var rules = await _twitterClient.StreamsV2.GetRulesForFilteredStreamV2Async();
        if (rules.Rules.Any())
        {
            await _twitterClient.StreamsV2.DeleteRulesFromFilteredStreamAsync(rules.Rules.Select(rule => rule.Id).ToArray());
        }

        // Add new rules
        var rule = new FilteredStreamRuleConfig("recommend #crypto lang:en", "English tweets about crypto");
        // await _twitterClient.StreamsV2.AddRulesToFilteredStreamAsync(rule);

        var currentAmount = 0;
        var stopped = false;

        var filteredStream = _twitterClient.StreamsV2.CreateFilteredStream();
        filteredStream.TweetReceived += async (sender, args) =>
        {
            try
            {
                currentAmount++;
                // await Task.WhenAll(
                //     _tweetHashtagService.AddHashtags(args.),
                //     _tweetGraphService.AddNodes(args),
                //     _tweetDbService.Store(args)
                // );
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


    public async Task ProcessFilteredStream2(int amount = 10)
    {
        var currentAmount = 0;
        var stopped = false;

        var filteredStream = _twitterClient.Streams.CreateFilteredStream();
        filteredStream.AddTrack("france", tweet =>
        {
            Console.WriteLine(tweet.Text);
        });
        filteredStream.EventReceived += async (sender, args) =>
        {
            try
            {
                currentAmount++;
                // await Task.WhenAll(
                //     _tweetHashtagService.AddHashtags(args.),
                //     _tweetGraphService.AddNodes(args),
                //     _tweetDbService.Store(args)
                // );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                if (currentAmount >= amount && !stopped)
                {
                    filteredStream.Stop();
                    stopped = true;
                }
            }
        };

        await filteredStream.StartMatchingAllConditionsAsync();
    }
}