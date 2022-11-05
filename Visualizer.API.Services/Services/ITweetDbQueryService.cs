using Visualizer.API.Services.Services.Impl;

namespace Visualizer.API.Services.Services;

public interface ITweetDbQueryService
{
    Task<TweetModelsPage> FindTweetsWithExpression(FindTweetsInputDto inputDto);
}
