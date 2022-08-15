// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Visualizer.Shared.Models;

namespace Visualizer.API.Clients;

public interface IIngestionClient
{
    Task<(HttpResponseMessage IsStreamingResponse, StreamingStatusDto streamingStatus)> IsStreamingRunning();
    Task<HttpResponseMessage> StartStreaming();
    Task<HttpResponseMessage> StopStreaming();
}
