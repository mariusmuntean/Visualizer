// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Redis.OM;

namespace Visualizer.Ingestion.Config;

public static class RedisOMConfig
{
    // ReSharper disable once InconsistentNaming
    public static void AddRedisOMConnectionProvider(this WebApplicationBuilder webApplicationBuilder)
    {
        var host = webApplicationBuilder.Configuration.GetSection("Redis")["Host"];
        var port = webApplicationBuilder.Configuration.GetSection("Redis")["Port"];
        var redisConnectionConfiguration = new RedisConnectionConfiguration
        {
            Host = host,
            Port = Convert.ToInt32(port)
        };
        var redisConnectionProvider = new RedisConnectionProvider(redisConnectionConfiguration);
        webApplicationBuilder.Services.AddSingleton(redisConnectionProvider);
    }
}
