using System.Reactive.Subjects;
using Visualizer.Services.Ingestion;

namespace Visualizer.HostedServices;

public class TweeterStreamingStarterService : IHostedService
{
    private readonly TwitterStreamService _twitterStreamService;
    private readonly ISubject<IsStreamingState> _isStreamingSubject = new ReplaySubject<IsStreamingState>(1);

    public TweeterStreamingStarterService(IServiceProvider serviceProvider)
    {
        var scope = serviceProvider.CreateScope();
        _twitterStreamService = scope.ServiceProvider.GetService<TwitterStreamService>();
    }

    public bool IsStreaming { get; private set; } = false;

    public IObservable<IsStreamingState> GetIsStreamingObservable() => _isStreamingSubject;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task<bool> StartChecking()
    {
        if (!IsStreaming)
        {
            if (await _twitterStreamService.ProcessSampleStream(Int32.MaxValue))
            {
                IsStreaming = true;
                _isStreamingSubject.OnNext(new IsStreamingState {IsStreaming = true});
            }
        }
        return IsStreaming;
    }

    public void StopChecking()
    {
        if (IsStreaming)
        {
            IsStreaming = false;
            _twitterStreamService.StopSampledStream();
            _isStreamingSubject.OnNext(new IsStreamingState {IsStreaming = false});
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public class IsStreamingState
    {
        public bool IsStreaming { get; set; }
    }
}
