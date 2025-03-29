using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace Globant.AspireDemo.AppHost;

internal static class ResourceBuilderExtensions
{
    internal static IResourceBuilder<T> WithSwaggerUI<T>(this IResourceBuilder<T> builder) where T : IResourceWithEndpoints =>
        builder.WithOpenApiDocs("swagger-ui-docs", "Swagger UI Documentation", "swagger");

    internal static IResourceBuilder<T> WithReDoc<T>(this IResourceBuilder<T> builder) where T : IResourceWithEndpoints =>
        builder.WithOpenApiDocs("redoc-docs", "ReDoc API Documentation", "api-docs");

    internal static IResourceBuilder<T> WithScalar<T>(this IResourceBuilder<T> builder) where T : IResourceWithEndpoints =>
        builder.WithOpenApiDocs("scalar-docs", "Scalar API Documentation", "scalar/v1");


    private static IResourceBuilder<T> WithOpenApiDocs<T>(this IResourceBuilder<T> builder,
        string name,
        string displayName,
        string openApiUiPath) where T : IResourceWithEndpoints
    {
        return builder.WithCommand(
            name,
            displayName,
            executeCommand: async _ =>
            {
                try
                {
                    var endpoint = builder.GetEndpoint("https");
                    var url = $"{endpoint.Url}/{openApiUiPath}";

                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

                    return new ExecuteCommandResult
                    {
                        Success = true,
                    };
                }
                catch (Exception e)
                {
                    return new ExecuteCommandResult
                    {
                        Success = false,
                        ErrorMessage = e.Message
                    };
                }
            },
            updateState: context => context.ResourceSnapshot.HealthStatus == HealthStatus.Healthy ?
                        ResourceCommandState.Enabled : ResourceCommandState.Disabled,
            iconName: "Document",
            iconVariant: IconVariant.Filled);
    }
}
