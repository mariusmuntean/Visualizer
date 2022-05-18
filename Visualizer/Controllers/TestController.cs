using Microsoft.AspNetCore.Mvc;
using Visualizer.Services;

namespace Visualizer.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly TweetBatchDownloadService _tweetBatchDownloadService;

    public TestController(TweetBatchDownloadService tweetBatchDownloadService)
    {
        _tweetBatchDownloadService = tweetBatchDownloadService;
    }

    [HttpPost("{amount}")]
    public async Task<IActionResult> BatchDownload([FromRoute] int amount)
    {
        await _tweetBatchDownloadService.BatchDownloadTweets(amount);
        return Ok();
    }
}