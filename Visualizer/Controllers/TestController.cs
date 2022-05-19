using Microsoft.AspNetCore.Mvc;
using Visualizer.Services;

namespace Visualizer.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly TwitterStreamService _twitterStreamService;

    public TestController(TwitterStreamService twitterStreamService)
    {
        _twitterStreamService = twitterStreamService;
    }

    [HttpPost("{amount}")]
    public async Task<IActionResult> BatchDownload([FromRoute] int amount)
    {
        await _twitterStreamService.ProcessSampleStream(amount);
        return Ok();
    }
}