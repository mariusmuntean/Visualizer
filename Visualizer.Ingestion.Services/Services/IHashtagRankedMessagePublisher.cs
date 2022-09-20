namespace Visualizer.Ingestion.Services.Services;

internal interface IHashtagRankedMessagePublisher
{
    Task PublishRankedHashtagMessage(string hashtag, int rank);
}
