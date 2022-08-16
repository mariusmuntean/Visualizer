// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using Tweetinvi.Events.V2;

namespace Visualizer.Ingestion.Services.Services;

public interface ITweetDbService
{
    Task Store(TweetV2ReceivedEventArgs tweetV2ReceivedEventArgs);
}
