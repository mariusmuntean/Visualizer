using Visualizer.Shared.Models;

namespace Visualizer.API.Services.Services;

public interface ITweetHashtagService
{
    IObservable<RankedHashtag> GetRankedHashtagObservable(double samplingIntervalSeconds);
    IObservable<RankedHashtag[]> GetTopRankedHashtagsObservable(int amount = 10);
    Task<RankedHashtag[]> GetTopHashtags(int amount = 10);
}
