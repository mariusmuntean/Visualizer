using Visualizer.API.Extensions;

namespace Visualizer.API;

public static class VisualizerRegistrator
{
    public static void Register(WebApplicationBuilder webApplicationBuilder)
    {
        // Add GraphQL
        webApplicationBuilder.AddVisualizerGraphQl();
    }
}
