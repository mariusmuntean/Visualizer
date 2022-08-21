using Visualizer.API.Services.DTOs;

namespace Visualizer.API.Services.Services;

public interface ITweetGraphService
{
    Task<GraphResultDto> GetNodes(int amount = 20);
    Task<long> CountUsers();
    Task<GraphResultDto> GetMentions(MentionFilterDto mentionFilterDto);
}
