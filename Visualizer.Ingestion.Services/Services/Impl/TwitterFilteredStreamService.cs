using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RedLockNet;
using Tweetinvi;
using Tweetinvi.Parameters.V2;
using Tweetinvi.Streaming.V2;
using Visualizer.Shared.Models;

namespace Visualizer.Ingestion.Services.Services.Impl;

internal class TwitterFilteredStreamService : ITwitterStreamService
{
    private readonly TwitterClient _twitterClient;
    private readonly ITweetGraphService _tweetGraphService;
    private readonly ITweetDbService _tweetDbService;
    private readonly ITweetHashtagService _tweetHashtagService;
    private readonly IDistributedLockFactory _iDistributedLockFactory;
    private readonly IStreamingStatusMessagePublisher _streamingStatusMessagePublisher;
    private readonly ILogger<TwitterFilteredStreamService> _logger;
    private IFilteredStreamV2? _filteredStream;

    public TwitterFilteredStreamService(TwitterClient twitterClient,
        ITweetGraphService tweetGraphService,
        ITweetDbService tweetDbService,
        ITweetHashtagService tweetHashtagService,
        IDistributedLockFactory iDistributedLockFactory,
        IStreamingStatusMessagePublisher streamingStatusMessagePublisher,
        ILogger<TwitterFilteredStreamService> logger)
    {
        _twitterClient = twitterClient;
        _tweetGraphService = tweetGraphService;
        _tweetDbService = tweetDbService;
        _tweetHashtagService = tweetHashtagService;
        _iDistributedLockFactory = iDistributedLockFactory;
        _streamingStatusMessagePublisher = streamingStatusMessagePublisher;
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

        // If the lock could not be acquired, then it means that another service instance is already processing the stream.
        if (!redLock.IsAcquired)
        {
            _logger.LogInformation("Couldn't acquire lock {TweetIngestionLock}, skipping ingestion", tweetIngestionLock);
            return;
        }

        _logger.LogInformation("Acquired lock {TweetIngestionLock}, starting ingestion", tweetIngestionLock);

        // If the lock could be acquired, then it means that no other service instance is processing the stream, so this instance can process it.

        var getRulesResponse = await _twitterClient.StreamsV2.GetRulesForFilteredStreamV2Async().ConfigureAwait(false);
        if (getRulesResponse?.Rules != null && getRulesResponse.Rules.Any())
        {
            // Delete rules
            var deleteRulesResponse = await _twitterClient.StreamsV2.DeleteRulesFromFilteredStreamAsync(getRulesResponse.Rules).ConfigureAwait(false);
        }

        // See https://developer.twitter.com/en/docs/twitter-api/tweets/filtered-stream/integrate/build-a-rule
        var addRulesResponse = await _twitterClient.StreamsV2.AddRulesToFilteredStreamAsync(new[]
        {
            new FilteredStreamRuleConfig("place_country:US","Tweets from the United States")
        }).ConfigureAwait(false);

        _filteredStream = _twitterClient.StreamsV2.CreateFilteredStream();
        _filteredStream.TweetReceived += async (sender, args) =>
        {
            try
            {
                await Task.WhenAll(
                    _tweetHashtagService.AddHashtags(args),
                    _tweetGraphService.AddNodes(args),
                    _tweetDbService.Store(args)
                ).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        };

        // Docs: https://developer.twitter.com/en/docs/twitter-api/tweets/volume-streams/api-reference/get-tweets-sample-stream#tab1
        var parameters = new StartFilteredStreamV2Parameters();
        parameters.Expansions.Add("geo.place_id");
        parameters.Expansions.Add("referenced_tweets.id");
        parameters.PlaceFields.Add("geo");
        parameters.TweetFields.Add("geo");

        _ = _filteredStream.StartAsync(parameters).ContinueWith((task, o) =>
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
        }, (redLock, _logger), TaskScheduler.Current);

        IsStreaming = true;
        _logger.LogInformation("Starting streaming with parameters: {Parameters}", JsonConvert.SerializeObject(parameters, Formatting.Indented));

        await PublishCurrentStreamStatusMessage().ConfigureAwait(false);
    }

    public async Task StopSampledStream()
    {
        if (!IsStreaming)
        {
            _logger.LogInformation("Streaming isn't running. Nothing to stop");
            return;
        }

        _filteredStream?.StopStream();
        IsStreaming = false;
        _logger.LogInformation("Stopped streaming");

        await PublishCurrentStreamStatusMessage().ConfigureAwait(false);
    }

    private async Task PublishCurrentStreamStatusMessage()
    {
        await _streamingStatusMessagePublisher.PublishStreamingStatus(new StreamingStatusDto {IsStreaming = IsStreaming}).ConfigureAwait(false);
    }
}
