﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Visualizer.Shared.Models;

namespace Visualizer.API.Clients;

internal class IngestionClient : IIngestionClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public IngestionClient(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<(HttpResponseMessage IsStreamingResponse, StreamingStatusDto streamingStatus)> IsStreamingRunning()
    {

        var response = await GetFreshClient().GetAsync("streaming").ConfigureAwait(false);
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
        var response = await GetFreshClient().PostAsync("streaming", stringContent).ConfigureAwait(false);
        return response;
    }

    private HttpClient GetFreshClient()
    {
        var ingestionServiceUrl = _configuration.GetSection("Ingestion")["Url"];
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(ingestionServiceUrl);
        return client;
    }
}
