using Soenneker.Jira.OpenApiClient;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Jira.OpenApiClientUtil.Abstract;

/// <summary>
/// Exposes a cached OpenAPI client instance.
/// </summary>
public interface IJiraOpenApiClientUtil: IDisposable, IAsyncDisposable
{
    /// <summary>Gets the cached client. Set Jira:ClientBaseUrl to the tenant origin for Basic authentication, or https://api.atlassian.com/ex/jira/{cloudId} for OAuth. Jira:AuthMode defaults to Basic using Jira:Email and Jira:ApiToken. Bearer mode reads Jira:AccessToken. Credentials are read for each request; OAuth acquisition and refresh belong to the caller.</summary>
    ValueTask<JiraOpenApiClient> Get(CancellationToken cancellationToken = default);
}
