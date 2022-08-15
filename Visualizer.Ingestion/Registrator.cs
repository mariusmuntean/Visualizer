// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Visualizer.Ingestion.Config;

namespace Visualizer.Ingestion;

public static class Registrator
{
    public static void RegisterServices(WebApplicationBuilder webApplicationBuilder)
    {
        // Register RedisGraph
        webApplicationBuilder.AddRedisGraph();
    }
}
