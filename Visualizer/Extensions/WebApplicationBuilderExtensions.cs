using System.Security.Claims;
using GraphQL;
using GraphQL.MicrosoftDI;
using NRedisGraph;
using Redis.OM;
using StackExchange.Redis;
using Tweetinvi;
using Tweetinvi.Models;
using GraphQL.DataLoader;
using GraphQL.Execution;
using GraphQL.MicrosoftDI;
using GraphQL.Server;
using GraphQL.Server.Authorization.AspNetCore;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Server.Ui.Altair;
using GraphQL.Server.Ui.GraphiQL;
using GraphQL.Server.Ui.Playground;
using GraphQL.Server.Ui.Voyager;
using GraphQL.SystemTextJson;
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
                options.EnableMetrics = true;
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