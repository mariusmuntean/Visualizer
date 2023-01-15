// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Visualizer.Ingestion.Migrations;

public interface IDataMigratorService
{
    Task MigrateData();
}

public class DataMigratorService : IDataMigratorService
{
    private readonly IServer _redisServer;
    private readonly ILogger<DataMigratorService> _logger;

    public DataMigratorService(IServer redisServer, ILogger<DataMigratorService> logger)
    {
        _redisServer = redisServer;
        _logger = logger;
    }

    public async Task MigrateData()
    {
        var scriptContent = @"import json
import random

gb = GearsBuilder()

# Map each element to a tuple of key and JSON doc.
gb.map(lambda x: (x['key'], json.loads(execute('JSON.GET', x['key'], '$'))))

# Add or update the Sentiment field with a computed value, possibly based on other data.
gb.map(lambda tup: execute('JSON.SET', tup[0], '$.Sentiment', ""\"""" + random.choice([""Negative"", ""Neutral"", ""Positive""]) + ""\""""))

gb.run('TweetModel:*')
";
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
                    _logger.LogError("Failed to perform migration");
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
}
