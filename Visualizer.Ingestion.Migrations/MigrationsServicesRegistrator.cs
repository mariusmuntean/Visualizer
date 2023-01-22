// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Visualizer.Ingestion.Migrations;

public static class MigrationsServicesRegistrator
{
    public static void Register(WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Services.AddSingleton<IDataMigratorService, DataMigratorService>();

        webApplicationBuilder.AddRedisServerAndConfigurator();
    }

    /// <summary>
    /// Adds an <see cref="IServer"/> instance to the DI container.
    /// </summary>
    /// <param name="webApplicationBuilder"></param>
    private static void AddRedisServerAndConfigurator(this WebApplicationBuilder webApplicationBuilder)
    {
        var host = webApplicationBuilder.Configuration.GetSection("Redis")["Host"];
        var port = webApplicationBuilder.Configuration.GetSection("Redis")["Port"];
        var configurationOptions = new ConfigurationOptions
        {
            EndPoints = new EndPointCollection {new DnsEndPoint(host, int.Parse(port))},
            SyncTimeout = 10000,
            AsyncTimeout = 10000,
            IncludePerformanceCountersInExceptions = true,
            IncludeDetailInExceptions = true,
            AllowAdmin = true
        };
        var muxer = ConnectionMultiplexer.Connect(configurationOptions);
        var iServer = muxer.GetServer(host, int.Parse(port));

        // register the iServer instance
        webApplicationBuilder.Services.AddSingleton(iServer);
    }
}
