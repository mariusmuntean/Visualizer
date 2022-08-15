using GraphQL.Server.Ui.Altair;
using GraphQL.Server.Ui.Voyager;
using Serilog;
using Serilog.Events;
using Visualizer.API.Clients;
using Visualizer.Extensions;
using Visualizer.GraphQl;
using Visualizer.HostedServices;
using Visualizer.Model;
using Visualizer.Services.Ingestion;
using Visualizer.Shared.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.local.json", true, true)
    .AddEnvironmentVariables()
    ;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddSingleton<IIngestionService, IngestionService>();
builder.Services.AddHostedService<IngestionService>(provider => (provider.GetService<IIngestionService>() as IngestionService)!);

// Add GraphQL
builder.AddVisualizerGraphQl();

// Add services for the Model project
ServiceRegistrator.Register(builder.Services);

// Add services for the Services project
Visualizer.Services.ServiceRegistrator.Register(builder.Services);

// Add services for the Clients project
ClientsRegistrator.Register(builder);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(); // Registers IHttpClientFactory - source https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-6.0#basic-usage
builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

// ToDo: call mapster
VisualizerMapster.Configure();

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

app.UseGraphQLVoyager(new VoyagerOptions {Headers = new Dictionary<string, object> {["MyHeader1"] = "MyValue", ["MyHeader2"] = 42,},});

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
