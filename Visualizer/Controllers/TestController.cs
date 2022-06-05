using Microsoft.AspNetCore.Mvc;
using Visualizer.HostedServices;
using Visualizer.Services;
using Visualizer.Services.Ingestion;

namespace Visualizer.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly TwitterStreamService _twitterStreamService;
    private readonly TweeterStreamingStarterService _tweeterStreamingStarterService;

    public TestController(TwitterStreamService twitterStreamService, TweeterStreamingStarterService tweeterStreamingStarterService)
    {
        _twitterStreamService = twitterStreamService;
        _tweeterStreamingStarterService = tweeterStreamingStarterService;
    }

    [HttpPost("{amount}")]
    public async Task<IActionResult> BatchDownload([FromRoute] int amount)
    {
        await _twitterStreamService.ProcessSampleStream(amount);
        return Ok();
    }

    [HttpPost("startStreaming")]
    public Task<IActionResult> StartStreaming()
    {
        _tweeterStreamingStarterService.StartChecking();
        return Task.FromResult<IActionResult>(Ok());
    }

    [HttpPost("stopStreaming")]
    public Task<IActionResult> StopStreaming()
    {
        _tweeterStreamingStarterService.StopChecking();
        return Task.FromResult<IActionResult>(Ok());
    }
}