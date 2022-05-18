using NRedisGraph;
using Redis.OM;
using StackExchange.Redis;
using Tweetinvi;
using Tweetinvi.Models;

namespace Visualizer.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddTwitterClient(this WebApplicationBuilder webApplicationBuilder)
    {
        var twitterSection = webApplicationBuilder.Configuration.GetSection("Twitter");
        var apiKey = twitterSection.GetValue<string>("ApiKey");
        var apiKeySecret = twitterSection.GetValue<string>("ApiKeySecret");
        var bearerToken = twitterSection.GetValue<string>("BearerToken");

        var appCreds = new ConsumerOnlyCredentials(apiKey, apiKeySecret)
        {
            BearerToken = bearerToken
        };
        var appClient = new TwitterClient(appCreds);
        webApplicationBuilder.Services.AddSingleton(appClient);
    }

    public static void AddRedisConnectionProvider(this WebApplicationBuilder webApplicationBuilder)
    {
        var connectionString = webApplicationBuilder.Configuration.GetSection("Redis")["ConnectionString"]
                               ?? throw new Exception("Cannot read Redis connection string");
        webApplicationBuilder.Services.AddSingleton(new RedisConnectionProvider(connectionString));
    }

    public static void AddRedisGraph(this WebApplicationBuilder webApplicationBuilder)
    {
        var muxer = ConnectionMultiplexer.Connect("localhost");
        var db = muxer.GetDatabase();
        var graph = new RedisGraph(db);

        webApplicationBuilder.Services.AddSingleton(db);
        webApplicationBuilder.Services.AddSingleton(graph);
    }
}