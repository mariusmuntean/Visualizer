using Tweetinvi;
using Tweetinvi.Models;

namespace Visualizer.Ingestion.Config;

public static class TwitterClientConfig
{
    public static void AddTwitterClient(this WebApplicationBuilder webApplicationBuilder)
    {
        var twitterSection = webApplicationBuilder.Configuration.GetSection("Twitter");
        var apiKey = twitterSection.GetValue<string>("ApiKey");
        var apiKeySecret = twitterSection.GetValue<string>("ApiKeySecret");
        var bearerToken = twitterSection.GetValue<string>("BearerToken");

        var appCreds = new ConsumerOnlyCredentials(apiKey, apiKeySecret) {BearerToken = bearerToken};
        var twitterClient = new TwitterClient(appCreds);
        webApplicationBuilder.Services.AddSingleton(twitterClient);
    }
}
