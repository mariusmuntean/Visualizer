using System.Globalization;
using Serilog;
using Visualizer.Ingestion;
using Visualizer.Ingestion.Migrations;
using Visualizer.Ingestion.Services;
using Visualizer.Shared.Models;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.local.json", true, true)
    .AddEnvironmentVariables()
    ;

// Add services to the container.
IngestionRegistrator.RegisterServices(builder);
IngestionServicesRegistrator.Register(builder);
MigrationsServicesRegistrator.Register(builder);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

VisualizerMapster.Configure();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.PerformDataMigration();

app.Run();
