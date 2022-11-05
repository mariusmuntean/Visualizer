using GraphQL;
using GraphQL.Execution;
using GraphQL.Validation;
using Visualizer.API.GraphQl;

namespace Visualizer.API.Config;

public static class GraphQlExtensions
{
    public static void AddVisualizerGraphQl(this WebApplicationBuilder webApplicationBuilder)
    {
        var services = webApplicationBuilder.Services;

        // Add GraphQL services and configure options
        services.AddGraphQL(builder => builder
            .UseApolloTracing()
            .AddSchema<VisualizerSchema>()
            .ConfigureExecutionOptions(options =>
            {
                options.EnableMetrics = false; // faster if disabled
                options.Listeners.Add(new LoggingDocExecListener());
                var logger = options.RequestServices.GetRequiredService<ILogger<Program>>();
                options.UnhandledExceptionDelegate = ctx =>
                {
                    logger.LogError("{Error} occurred", ctx.OriginalException.Message);
                    return Task.CompletedTask;
                };
            })
            .AddSystemTextJson()
            .AddDataLoader()
            .AddGraphTypes(typeof(VisualizerSchema).Assembly)
        );
    }

    private class LoggingDocExecListener : IDocumentExecutionListener
    {
        public Task AfterValidationAsync(IExecutionContext context, IValidationResult validationResult)
        {
            return Task.CompletedTask;
        }

        public Task BeforeExecutionAsync(IExecutionContext context)
        {
            return Task.CompletedTask;
        }

        public Task AfterExecutionAsync(IExecutionContext context)
        {
            return Task.CompletedTask;
        }
    }
}
