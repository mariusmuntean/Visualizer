using Microsoft.AspNetCore.Mvc;
using Visualizer.Ingestion.Services.Services;
using Visualizer.Shared.Models;

namespace Visualizer.Ingestion.Controllers;

[ApiController]
[Route("streaming")]
public class StreamingController : ControllerBase
{
    private readonly ITwitterStreamService _twitterStreamService;

    public StreamingController(ITwitterStreamService twitterStreamService)
    {
        _twitterStreamService = twitterStreamService;
    }

    [Produces("application/json")]
    [HttpGet]
    public IActionResult GetStreamingStatus()
    {
        var isStreaming = _twitterStreamService.IsStreaming;
        return Ok(new StreamingStatusDto {IsStreaming = isStreaming});
    }

    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> ChangeStreamingStatus([FromBody] StreamingCommand streamingCommand)
    {
        await (streamingCommand.ShouldRun switch
        {
            true => _twitterStreamService.ProcessSampleStream(),
            false => _twitterStreamService.StopSampledStream()
        }).ConfigureAwait(false);

        return Ok();
    }
}
