using Redis.OM;
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
}