﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Net;
using NRedisGraph;
using StackExchange.Redis;

namespace Visualizer.Ingestion.Config;

public static class RedisGraphConfig
{
    public static void AddRedisGraph(this WebApplicationBuilder webApplicationBuilder)
    {
        var host = webApplicationBuilder.Configuration.GetSection("Redis")["Host"];
        var port = webApplicationBuilder.Configuration.GetSection("Redis")["Port"];
        var configurationOptions = new ConfigurationOptions
        {
            EndPoints = new EndPointCollection {new DnsEndPoint(host, int.Parse(port))},
            SyncTimeout = 10000,
            AsyncTimeout = 10000,
            IncludePerformanceCountersInExceptions = true,
            IncludeDetailInExceptions = true
        };
        var muxer = ConnectionMultiplexer.Connect(configurationOptions);
        var db = muxer.GetDatabase();
        var graph = new RedisGraph(db);

        webApplicationBuilder.Services.AddSingleton(db); // ToDo: necessary?
        webApplicationBuilder.Services.AddSingleton(graph);
    }
}
