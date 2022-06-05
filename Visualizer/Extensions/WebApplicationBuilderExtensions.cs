using System.Security.Claims;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.MicrosoftDI;
using GraphQL.Server;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.SystemTextJson;
using NRedisGraph;
using Redis.OM;
using StackExchange.Redis;
using Tweetinvi;
using Tweetinvi.Models;
using Visualizer.GraphQl;

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
        var redisConnectionProvider = new RedisConnectionProvider(connectionString);
        webApplicationBuilder.Services.AddSingleton(redisConnectionProvider);
    }

    public static void AddRedisGraph(this WebApplicationBuilder webApplicationBuilder)
    {
        // var muxer = ConnectionMultiplexer.Connect("localhost");
        var configurationOptions = ConfigurationOptions.Parse("localhost");
        configurationOptions.SyncTimeout = 10000;
        configurationOptions.AsyncTimeout = 10000;
        configurationOptions.IncludePerformanceCountersInExceptions = true;
        configurationOptions.IncludeDetailInExceptions = true;
        var muxer = ConnectionMultiplexer.Connect(configurationOptions);
        var db = muxer.GetDatabase();
        var graph = new RedisGraph(db);

        webApplicationBuilder.Services.AddSingleton(db);
        webApplicationBuilder.Services.AddSingleton(graph);
    }


    public static void AddVisualizerGraphQl(this WebApplicationBuilder webApplicationBuilder)
    {
        var services = webApplicationBuilder.Services;

        // Add GraphQL services and configure options
        services.AddSingleton<VisualizerSchema>();
        services.AddSingleton<GraphQLHttpMiddleware<VisualizerSchema>>();
        services.AddGraphQL(builder => builder
            .AddApolloTracing()
            .AddWebSocketsHttpMiddleware<VisualizerSchema>()
            
            .AddSchema<VisualizerSchema>()
            .ConfigureExecutionOptions(options =>
            {
                options.EnableMetrics = false; // faster if disabled
                var logger = options.RequestServices.GetRequiredService<ILogger<Program>>();
                options.UnhandledExceptionDelegate = ctx =>
                {
                    logger.LogError("{Error} occurred", ctx.OriginalException.Message);
                    return Task.CompletedTask;
                };
            })
            .AddSystemTextJson()
            .AddWebSockets()
            .AddDataLoader()
            .AddGraphTypes(typeof(VisualizerSchema).Assembly));
    }

    public class GraphQLUserContext : Dictionary<string, object>
    {
        public ClaimsPrincipal User { get; set; }
    }
}