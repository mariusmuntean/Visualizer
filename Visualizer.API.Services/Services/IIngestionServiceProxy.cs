using Microsoft.Extensions.Hosting;
using Visualizer.Shared.Models;

namespace Visualizer.API.Services.Services;

public interface IIngestionServiceProxy : IHostedService
{
    Task<bool> IsStreaming();
    Task<HttpResponseMessage> StartStreaming();
    Task<HttpResponseMessage> StopStreaming();
    IObservable<StreamingStatusDto> GetStreamingStatusObservable();
}
