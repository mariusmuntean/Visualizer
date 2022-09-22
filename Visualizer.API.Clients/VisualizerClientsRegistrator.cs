using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Visualizer.API.Clients.Impl;

namespace Visualizer.API.Clients;

public static class VisualizerClientsRegistrator
{
    public static void Register(WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Services.AddSingleton<IIngestionClient, IngestionClient>();
    }
}
