using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Redis.OM;

namespace Visualizer.API.Services.Config;

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
