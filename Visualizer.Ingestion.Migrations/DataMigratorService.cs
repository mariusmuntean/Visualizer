using Microsoft.Extensions.Logging;
using RedLockNet;
using StackExchange.Redis;

namespace Visualizer.Ingestion.Migrations;

class DataMigratorService : IDataMigratorService
{
    private const string PerformedMigrationsKey = "PerformedMigrations";
    private const string PerformMigrationsLock = "PerformMigrationsLock";

    private readonly IServer _redisServer;
    private readonly IDatabase _database;
    private readonly IDistributedLockFactory _distributedLockFactory;
    private readonly ILogger<DataMigratorService> _logger;

    public DataMigratorService(IServer redisServer, IDatabase database, IDistributedLockFactory distributedLockFactory, ILogger<DataMigratorService> logger)
    {
        _redisServer = redisServer;
        _database = database;
        _distributedLockFactory = distributedLockFactory;
        _logger = logger;
    }

    public async Task MigrateData()
    {
        // Multiple service instances might try to perform the migrations.
        // Only one should be performing the changes. The others should be blocked until the migrations have been performed and it is safe to work with the data again.
        // I'm using the RedLock distributed lock algorithm - https://github.com/samcook/RedLock.net - which guarantees that everyone can eventually acquire the lock (deadlock free).

        // Try to obtain a distributed lock and block until the lock is acquired - Note that 'await using' that disposes the lock when this method finishes..
        var expiryTime = TimeSpan.FromSeconds(30); // Expiry time when the service crashes
        var waitTime = TimeSpan.FromTicks(long.MaxValue); // Wait 'forever' to acquire the lock. Maybe use the optional cancellation token to cancel the wait.
        var retryTime = TimeSpan.FromSeconds(2);
        await using var performMigrationsLock = await _distributedLockFactory.CreateLockAsync(PerformMigrationsLock, expiryTime, waitTime, retryTime);
        if (!performMigrationsLock.IsAcquired)
        {
            // This should never happen so let the service instance crash and burn.
            throw new Exception("Couldn't acquire distributed lock for performing the data migrations. This should not happen...");
        }

        // Perform migrations
        var dataMigrationScriptsToExecute = await GetMigrationScriptsToExecute();
        if (!dataMigrationScriptsToExecute.Any())
        {
            _logger.LogInformation("No data migration scripts to run");
            return;
        }

        foreach (var dataMigrationScriptName in dataMigrationScriptsToExecute)
        {
            _logger.LogInformation("Executing data migration script {ScriptName}", dataMigrationScriptName);

            // Get data migration script content
            var scriptContent = await GetDataMigrationScriptContent(dataMigrationScriptName);

            // Run script
            await RunRedisGearsPythonScript(scriptContent);

            // Record the name of the script
            await _database.SetAddAsync(PerformedMigrationsKey, dataMigrationScriptName);

            _logger.LogInformation("Successfully executed data migration script {ScriptName}", dataMigrationScriptName);
        }
    }

    private async Task RunRedisGearsPythonScript(string scriptContent)
    {
        var redisResult = await _redisServer.Multiplexer.GetDatabase(0).ExecuteAsync("RG.PYEXECUTE", scriptContent).ConfigureAwait(false);
        switch (redisResult.Type)
        {
            case ResultType.Error:
            {
                throw new Exception($"Failed to perform data migration: {redisResult.ToString()}");
            }
            case ResultType.MultiBulk:
            {
                var redisValue = (RedisResult[]) redisResult;
                var resultValue = (RedisValue[]) redisValue[0];
                var errorValue = (RedisValue[]) redisValue[1];

                if (errorValue is not null && errorValue.Length > 0)
                {
                    throw new Exception($"Failed to perform migration. One or more error occurred: {string.Join(Environment.NewLine, errorValue.Select(e => e.ToString()))}");
                }
                else
                {
                    _logger.LogInformation("Data migration done. Affected {Amount} documents", resultValue.Length);
                }

                break;
            }
            default:
            {
                var redisValue = (RedisValue) redisResult;
                _logger.LogInformation("Data migration done: {RedisValue}", redisValue);
                break;
            }
        }
    }

    private async Task<string> GetDataMigrationScriptContent(string dataMigrationScriptName)
    {
        var scriptPath = Path.Join(GetDataMigrationsDirectoryPath(), dataMigrationScriptName);
        _logger.LogInformation("Reading content of data migration script: {ScriptPath}", scriptPath);

        return await File.ReadAllTextAsync(scriptPath);
    }

    /// <summary>
    /// Decides the names of the migration scripts that still need to be executed.
    /// Note: execute the scripts exactly in the order that they're returned by this method.
    /// </summary>
    /// <returns>An array of the migration scripts to be executed.</returns>
    /// <exception cref="Exception">If more scripts were already executed than are actually available.</exception>
    /// <exception cref="Exception">If scripts were executed that aren't actually available.</exception>
    private async Task<string[]> GetMigrationScriptsToExecute()
    {
        // Determine all available migration scripts
        var allAvailableDataMigrationScripts = GetAllAvailableDataMigrationScriptNames();
        _logger.LogInformation("Found these data migration scripts {Scripts}", string.Join(Environment.NewLine, allAvailableDataMigrationScripts.ToArray().OrderBy(s => s)));

        // Determine already executed migration scripts
        var executedMigrationScripts = await _database.SetMembersAsync(PerformedMigrationsKey);
        var executedMigrationScriptNames = executedMigrationScripts.Select(v => v.ToString()).ToArray();
        _logger.LogInformation("Already executed these data migration scripts {Scripts}", string.Join(Environment.NewLine, executedMigrationScriptNames.ToArray().OrderBy(s => s)));

        // Sanity checks
        if (executedMigrationScriptNames.Length > allAvailableDataMigrationScripts.Count)
        {
            throw new Exception($"More scripts executed ({executedMigrationScriptNames.Length}) than available ({allAvailableDataMigrationScripts})");
        }

        var executedButUnknownScriptNames = executedMigrationScriptNames.Except(allAvailableDataMigrationScripts).OrderBy(s => s).ToArray();
        if (executedButUnknownScriptNames.Any())
        {
            throw new Exception($"The following executed scripts are unknown: {string.Join(Environment.NewLine, executedButUnknownScriptNames)}");
        }

        // Determine which scripts have to be executed and sort them in ascending order
        return allAvailableDataMigrationScripts.Except(executedMigrationScriptNames).OrderBy(s => s).ToArray();
    }

    private HashSet<string> GetAllAvailableDataMigrationScriptNames()
    {
        var dataMigrationsPath = GetDataMigrationsDirectoryPath();
        var dataMigrationScripts = Directory.GetFiles(dataMigrationsPath);
        return dataMigrationScripts.Select(scriptPath => Path.GetFileName(scriptPath)).ToHashSet();
    }

    private string GetDataMigrationsDirectoryPath()
    {
        var currentDirPath = Directory.GetParent(this.GetType().Assembly.Location);
        var dataMigrationsPath = Path.Join(currentDirPath.FullName, "DataMigration");
        return dataMigrationsPath;
    }
}
