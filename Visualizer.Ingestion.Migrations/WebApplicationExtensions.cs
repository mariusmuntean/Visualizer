using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Visualizer.Ingestion.Migrations;

public static class WebApplicationExtensions
{
    public static async Task PerformDataMigration(this WebApplication app)
    {
        var service = app.Services.GetRequiredService<IDataMigratorService>();
        await service.MigrateData();
    }
}
