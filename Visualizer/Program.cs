using Visualizer.Extensions;
using Visualizer.HostedServices;
using Visualizer.Model;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", true, true);

// Add services to the container.

// Add TwitterClient
builder.AddTwitterClient();

// Add RedisConnectionProvider
builder.AddRedisConnectionProvider();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services for the Model project
ServiceRegistrator.Register(builder.Services);

// Add services for the Services project
Visualizer.Services.ServiceRegistrator.Register(builder.Services);

// Add hosted services
builder.Services.AddHostedService<IndexInitializer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();