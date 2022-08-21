using Visualizer.API.Services.DTOs;

namespace Visualizer.API.Services.Services;

public interface ITweetHashtagService
{
    IObservable<ScoredHashtag> GetHashtagAddedObservable();
    IObservable<ScoredHashtag[]> GetRankedHashtagsObservable(int amount = 10);
    Task<ScoredHashtag[]> GetTopHashtags(int amount = 10);
}
