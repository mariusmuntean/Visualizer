using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RedLockNet;
using Tweetinvi;
using Tweetinvi.Parameters.V2;
using Tweetinvi.Streaming.V2;

namespace Visualizer.Ingestion.Services;

public class TwitterStreamService
{
    private readonly TwitterClient _twitterClient;
    private readonly TweetGraphService _tweetGraphService;
    private readonly TweetDbService _tweetDbService;
    private readonly TweetHashtagService _tweetHashtagService;
    private readonly IDistributedLockFactory _iDistributedLockFactory;
    private readonly ILogger<TwitterStreamService> _logger;
    private ISampleStreamV2? _sampleStream;

    public TwitterStreamService(TwitterClient twitterClient,
        TweetGraphService tweetGraphService,
        TweetDbService tweetDbService,
        TweetHashtagService tweetHashtagService,
        IDistributedLockFactory iDistributedLockFactory,
        ILogger<TwitterStreamService> logger)
    {
        _twitterClient = twitterClient;
        _tweetGraphService = tweetGraphService;
        _tweetDbService = tweetDbService;
        _tweetHashtagService = tweetHashtagService;
        _iDistributedLockFactory = iDistributedLockFactory;
        _logger = logger;
    }

    public bool IsStreaming { get; private set; }

    public async Task ProcessSampleStream()
    {
        if (IsStreaming)
        {
            _logger.LogInformation("Streaming is already running. Nothing to start");
            return;
        }

        var tweetIngestionLock = "tweet_ingestion";
        var lockExpiryTime = TimeSpan.FromSeconds(10);
        var acquireLockDuration = TimeSpan.FromSeconds(10); // 10 seconds to acquire the lock, afterwards it gives up
        var acquireLockRetryInterval = TimeSpan.FromSeconds(5); // 5 seconds to wait before retrying to acquire the lock
        var redLock = await _iDistributedLockFactory.CreateLockAsync(tweetIngestionLock, lockExpiryTime, acquireLockDuration, acquireLockRetryInterval);

        // If the lock is not acquired, then it means that another service instance is already processing the stream.
        if (!redLock.IsAcquired)
        {
            _logger.LogInformation("Couldn't acquire lock {TweetIngestionLock}, skipping ingestion", tweetIngestionLock);
            return;
        }

        _logger.LogInformation("Acquired lock {TweetIngestionLock}, starting ingestion", tweetIngestionLock);

        // If the lock is acquired, then it means that no other service instance is processing the stream, so this instance can process it.

        _sampleStream = _twitterClient.StreamsV2.CreateSampleStream();
        _sampleStream.TweetReceived += async (sender, args) =>
        {
            try
            {
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
        };

        // Docs: https://developer.twitter.com/en/docs/twitter-api/tweets/volume-streams/api-reference/get-tweets-sample-stream#tab1
        var parameters = new StartSampleStreamV2Parameters();
        parameters.Expansions.Add("geo.place_id");
        parameters.Expansions.Add("referenced_tweets.id");
        parameters.PlaceFields.Add("geo");
        parameters.TweetFields.Add("geo");

        _ = _sampleStream.StartAsync(parameters)
            .ContinueWith((task, o) =>
            {
                var state = o as (IRedLock l, ILogger<TwitterStreamService> log)?;
                if (state is null)
                {
                    return;
                }

                var (l, log) = state.Value;
                log.LogInformation("Stopped streaming tweets");

                if (!task.IsCompletedSuccessfully)
                {
                    log.LogError(task.Exception, "Tweet streaming failed");
                }

                l.Dispose();
                _logger.LogInformation("Released lock {TweetIngestionLock}, stopping ingestion", tweetIngestionLock);
            }, (redLock, _logger));

        IsStreaming = true;
        _logger.LogInformation("Starting streaming with parameters: {Parameters}", JsonConvert.SerializeObject(parameters, Formatting.Indented));
    }

    public void StopSampledStream()
    {
        if (!IsStreaming)
        {
            _logger.LogInformation("Streaming isn't running. Nothing to stop");
            return;
        }

        _sampleStream?.StopStream();
        IsStreaming = false;
        _logger.LogInformation("Stopped streaming");
    }
}
