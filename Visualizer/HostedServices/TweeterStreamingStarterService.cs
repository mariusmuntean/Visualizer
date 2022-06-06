using System.Reactive.Linq;
using System.Reactive.Subjects;
using Visualizer.Services.Ingestion;

namespace Visualizer.HostedServices;

public class TweeterStreamingStarterService : IHostedService
{
    private readonly TwitterStreamService _twitterStreamService;
    private bool _isStreaming = false;
    private readonly ISubject<IsStreamingState> _isStreamingSubject = new ReplaySubject<IsStreamingState>(1);

    public TweeterStreamingStarterService(IServiceProvider serviceProvider)
    {
        var scope = serviceProvider.CreateScope();
        _twitterStreamService = scope.ServiceProvider.GetService<TwitterStreamService>();
    }

    public bool IsStreaming
    {
        get => _isStreaming;
        private set
        {
            _isStreaming = value;
        }
    }

    public IObservable<IsStreamingState> GetIsStreamingObservable() => _isStreamingSubject;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void StartChecking()
    {
        if (!IsStreaming)
        {
            IsStreaming = true;
            _twitterStreamService?.ProcessSampleStream(Int32.MaxValue);
            _isStreamingSubject.OnNext(new IsStreamingState {IsStreaming = true});
        }
    }

    public void StopChecking()
    {
        if (IsStreaming)
        {
            IsStreaming = false;
            _twitterStreamService?.StopSampledStream();
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