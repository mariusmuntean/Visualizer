namespace Visualizer.Ingestion.Migrations;

public interface IDataMigratorService
{
    Task MigrateData();
}
