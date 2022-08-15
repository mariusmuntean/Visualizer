using Microsoft.AspNetCore.Mvc;
using Visualizer.Ingestion.Services;
using Visualizer.Shared.Models;

namespace Visualizer.Ingestion.Controllers;

[ApiController]
[Route("streaming")]
public class StreamingController : ControllerBase
{
    private readonly TwitterStreamService _twitterStreamService;

    public StreamingController(TwitterStreamService twitterStreamService)
    {
        _twitterStreamService = twitterStreamService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStreamingStatus()
    {
        var isStreaming = _twitterStreamService.IsStreaming;
        return Ok(new StreamingStatusDto() {IsStreaming = isStreaming});
    }

    [HttpPost]
    public async Task<IActionResult> ChangeStreamingStatus([FromBody] StreamingCommand streamingCommand)
    {
        if (streamingCommand.ShouldRun)
        {
            await _twitterStreamService.ProcessSampleStream().ConfigureAwait(false);
        }
        else
        {
            _twitterStreamService.StopSampledStream();
        }

        return Ok();
    }
}
