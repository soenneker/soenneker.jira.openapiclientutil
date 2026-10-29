using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Jira.HttpClients.Registrars;
using Soenneker.Jira.OpenApiClientUtil.Abstract;

namespace Soenneker.Jira.OpenApiClientUtil.Registrars;

/// <summary>
/// Registers the OpenAPI client utility for dependency injection.
/// </summary>
public static class JiraOpenApiClientUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="JiraOpenApiClientUtil"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddJiraOpenApiClientUtilAsSingleton(this IServiceCollection services)
    {
        services.AddJiraOpenApiHttpClientAsSingleton()
                .TryAddSingleton<IJiraOpenApiClientUtil, JiraOpenApiClientUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="JiraOpenApiClientUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddJiraOpenApiClientUtilAsScoped(this IServiceCollection services)
    {
        services.AddJiraOpenApiHttpClientAsSingleton()
                .TryAddScoped<IJiraOpenApiClientUtil, JiraOpenApiClientUtil>();

        return services;
    }
}
