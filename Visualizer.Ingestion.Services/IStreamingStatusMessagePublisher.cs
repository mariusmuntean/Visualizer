// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Visualizer.Shared.Models;

namespace Visualizer.Ingestion.Services;

public interface IStreamingStatusMessagePublisher
{
    Task PublishStreamingStatus(StreamingStatusDto streamingStatusDto);
}
