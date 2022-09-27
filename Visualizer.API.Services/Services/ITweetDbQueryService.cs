using Visualizer.API.Services.Services.Impl;
using Visualizer.Shared.Models;

namespace Visualizer.API.Services.Services;

public interface ITweetDbQueryService
{
    Task<List<TweetModel>> FindTweets(FindTweetsInputDto inputDto);
    Task<TweetModelsPage> FindTweetsWithExpression(FindTweetsInputDto inputDto);
}
