using Visualizer.API.Config;

namespace Visualizer.API;

// ReSharper disable once InconsistentNaming
public static class VisualizerAPIRegistrator
{
    public static void Register(WebApplicationBuilder webApplicationBuilder)
    {
        // Add GraphQL
        webApplicationBuilder.AddVisualizerGraphQl();
    }
}
