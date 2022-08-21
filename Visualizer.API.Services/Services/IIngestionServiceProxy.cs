using Microsoft.Extensions.Hosting;
using Visualizer.Shared.Models;

namespace Visualizer.API.Services.Services;

public interface IIngestionServiceProxy : IHostedService
{
    bool? IsStreaming { get; }
    Task<HttpResponseMessage> StartStreaming();
    Task<HttpResponseMessage> StopStreaming();
    IObservable<StreamingStatusDto> GetStreamingStatusObservable();
}
