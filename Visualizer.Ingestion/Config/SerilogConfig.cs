using Serilog;
using Serilog.Events;

namespace Visualizer.Ingestion.Config;

public static class SerilogConfig
{
    public static void AddVisualizerSerilog(this WebApplicationBuilder webApplicationBuilder)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
        
        webApplicationBuilder.Host.UseSerilog();
    }
}
