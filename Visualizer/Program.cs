using GraphQL.Server.Ui.Altair;
using GraphQL.Server.Ui.Voyager;
using Mapster;
using Redis.OM.Modeling;
using Serilog;
using Serilog.Events;
using Tweetinvi.Core.Models;
using Tweetinvi.Models.V2;
using Visualizer.Extensions;
using Visualizer.GraphQl;
using Visualizer.HostedServices;
using Visualizer.Model;
using Visualizer.Model.TweetDb;

const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", true, true);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container.

// Add GraphQL
builder.AddVisualizerGraphQl();

// Add TwitterClient
builder.AddTwitterClient();

// Add RedisConnectionProvider
builder.AddRedisConnectionProvider();

// Add RedisGraph
builder.AddRedisGraph();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services for the Model project
ServiceRegistrator.Register(builder.Services);

// Add services for the Services project
Visualizer.Services.ServiceRegistrator.Register(builder.Services);

// Add my services
builder.Services.AddHostedService<IndexInitializer>();
builder.Services.AddHostedService<GraphInitializer>();
builder.Services.AddSingleton<TweeterStreamingStarterService>();
builder.Services.AddHostedService<TweeterStreamingStarterService>(provider => provider.GetService<TweeterStreamingStarterService>());

builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

// Config Mapster
TypeAdapterConfig<DateTimeOffset, DateTime>.NewConfig()
    .MapWith(offset => offset.DateTime)
    ;
TypeAdapterConfig<TweetV2, TweetModel>.NewConfig()
    .Map(dest => dest.Id, src => src.Id)
    .Map(dest => dest.CreatedAt, src => src.CreatedAt.UtcTicks)
    .Map(dest => dest.GeoLoc,
    src => new GeoLoc(src.Geo.Coordinates.Coordinates[0], src.Geo.Coordinates.Coordinates[1]),
    src => src.Geo.Coordinates != null && src.Geo.Coordinates.Coordinates != null)
    ;
TypeAdapterConfig<TweetEntitiesV2, TweetEntities>.NewConfig()
    .Map(dest => dest.Hashtags, src => src.Hashtags.Select(h => h.Tag), src => src.Hashtags != null)
    .Map(dest => dest.Cashtags, src => src.Cashtags.Select(c => c.Tag), src => src.Cashtags != null)
    .Map(dest => dest.Mentions, src => src.Mentions.Select(m => m.Username), src => src.Mentions != null)
    ;

// global cors policy
app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    // .WithOrigins("http://localhost:3000")
    .SetIsOriginAllowed(origin => true) // allow any origin
    .AllowCredentials()); // allow credentials

// GraphQL
app.UseWebSockets();
app.UseGraphQL<VisualizerSchema>();
app.UseGraphQLWebSockets<VisualizerSchema>();

app.UseGraphQLAltair(new AltairOptions
{
    // Headers = new Dictionary<string, string>
    // {
    //     ["X-api-token"] = "130fh9823bd023hd892d0j238dh",
    // }
});

app.UseGraphQLVoyager(new VoyagerOptions
{
    Headers = new Dictionary<string, object>
    {
        ["MyHeader1"] = "MyValue",
        ["MyHeader2"] = 42,
    },
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Try to run the app
try
{
    Log.Information("Starting web host");
    app.Run();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
