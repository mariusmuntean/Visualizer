using Visualizer.API.Services.DTOs;
using Visualizer.Shared.Models;

namespace Visualizer.API.Services.Services;

public interface ITweetHashtagService
{
    IObservable<RankedHashtag> GetHashtagAddedObservable();
    IObservable<RankedHashtag[]> GetRankedHashtagsObservable(int amount = 10);
    Task<RankedHashtag[]> GetTopHashtags(int amount = 10);
}
