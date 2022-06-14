using GraphQL.Server.Ui.Altair;
using GraphQL.Server.Ui.Voyager;
using Mapster;
using Redis.OM.Modeling;
using Tweetinvi.Core.Models;
using Visualizer.Extensions;
using Visualizer.GraphQl;
using Visualizer.HostedServices;
using Visualizer.Model;

const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", true, true);

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

// Config Mapster
TypeAdapterConfig<DateTimeOffset, DateTime>.NewConfig()
    .MapWith(offset => offset.DateTime)
    ;
TypeAdapterConfig<Tweet, TweetModel>.NewConfig()
    .Map(dest => dest.Id, src => src.Id)
    // .Map(dest => dest.CreatedAt, src => src.CreatedAt.DateTime)
    .Map(dest => dest.GeoLoc, src => new GeoLoc(src.Coordinates.Latitude, src.Coordinates.Longitude))
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

app.Run();