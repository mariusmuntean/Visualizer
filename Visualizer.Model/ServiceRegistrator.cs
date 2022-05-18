using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Redis.OM;

namespace Visualizer.Model;

public static class ServiceRegistrator
{
    public static void Register(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(provider =>
        {
            var connectionString = provider.GetService<IConfiguration>()
                                       ?.GetSection("Redis")["ConnectionString"]
                                   ?? throw new Exception("Cannot read Redis connection string");
            return new RedisConnectionProvider(connectionString);
        });
    }
}