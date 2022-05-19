using Visualizer.Services;

namespace Visualizer.HostedServices;

public class TweeterStreamingStarterService : IHostedService
{
    private readonly TwitterStreamService _twitterStreamService;
    private bool _isStreaming = false;

    public TweeterStreamingStarterService(IServiceProvider serviceProvider)
    {
        var scope = serviceProvider.CreateScope();
        _twitterStreamService = scope.ServiceProvider.GetService<TwitterStreamService>();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void StartChecking()
    {
        if (!_isStreaming)
        {
            _isStreaming = true;
            _twitterStreamService?.ProcessSampleStream(Int32.MaxValue);
        }
    }

    public void StopChecking()
    {
        if (_isStreaming)
        {
            _isStreaming = false;
            _twitterStreamService?.StopSampledStream();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}