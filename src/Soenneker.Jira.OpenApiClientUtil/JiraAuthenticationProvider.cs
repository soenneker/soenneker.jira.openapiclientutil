using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Soenneker.Jira.OpenApiClientUtil;

internal sealed class JiraAuthenticationProvider(IConfiguration configuration, Uri baseAddress) : IAuthenticationProvider
{
    public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Uri uri = request.URI;
        if (uri.Scheme != Uri.UriSchemeHttps || !uri.Authority.Equals(baseAddress.Authority, StringComparison.OrdinalIgnoreCase) ||
            !uri.AbsolutePath.StartsWith(baseAddress.AbsolutePath.TrimEnd('/') + "/", StringComparison.Ordinal))
            throw new InvalidOperationException("Jira credentials may only be sent to the configured HTTPS API base URL.");


        string mode = configuration["Jira:AuthMode"] ?? "Basic";
        string authorization;
        if (mode.Equals("Basic", StringComparison.OrdinalIgnoreCase))
        {
            string email = Required("Jira:Email");
            if (email.Contains(':'))
                throw new InvalidOperationException("Jira:Email cannot contain a colon.");
            authorization = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(email + ":" + Required("Jira:ApiToken")));
        }
        else if (mode.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            authorization = "Bearer " + Required("Jira:AccessToken");
        else
            throw new InvalidOperationException("Jira:AuthMode must be Basic or Bearer.");

        request.Headers.Remove("Authorization");
        request.Headers.Add("Authorization", authorization);

        return Task.CompletedTask;
    }

    private string Required(string key)
    {
        string? value = configuration[key];
        return !string.IsNullOrWhiteSpace(value) ? value
            : throw new InvalidOperationException(key + " is required.");
    }
}
