// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Visualizer.Shared.Models;

namespace Visualizer.API.Services.Ingestion;

public interface IIngestionService
{
    bool? IsStreaming { get; }
    Task<HttpResponseMessage> StartStreaming();
    Task<HttpResponseMessage> StopStreaming();
    IObservable<StreamingStatusDto> GetStreamingStatusObservable();
}
