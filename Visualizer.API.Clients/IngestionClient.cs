using Newtonsoft.Json;
using Visualizer.Shared.Models;

namespace Visualizer.API.Clients;

public class IngestionClient : IIngestionClient
{
    private readonly HttpClient _httpClient;

    public IngestionClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(HttpResponseMessage IsStreamingResponse, StreamingStatusDto streamingStatus)> IsStreamingRunning()
    {
        var response = await _httpClient.GetAsync("streaming").ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            var streamingStatusStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return (response, JsonConvert.DeserializeObject<StreamingStatusDto>(streamingStatusStr));
        }

        return (response, null);
    }

    public async Task<HttpResponseMessage> StartStreaming()
    {
        var streamingCommand = new StreamingCommand(true);
        var response = await SendStreamingCommand(streamingCommand).ConfigureAwait(false);
        return response;
    }

    public async Task<HttpResponseMessage> StopStreaming()
    {
        var streamingCommand = new StreamingCommand(false);
        var response = await SendStreamingCommand(streamingCommand).ConfigureAwait(false);
        return response;
    }

    private async Task<HttpResponseMessage> SendStreamingCommand(StreamingCommand streamingCommand)
    {
        var streamingCommandStr = JsonConvert.SerializeObject(streamingCommand);
        var stringContent = new StringContent(streamingCommandStr);
        var response = await _httpClient.PostAsync("streaming", stringContent).ConfigureAwait(false);
        return response;
    }
}
