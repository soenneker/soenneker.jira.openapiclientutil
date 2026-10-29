using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Soenneker.Extensions.ValueTask;
using Soenneker.Jira.HttpClients.Abstract;
using Soenneker.Jira.OpenApiClientUtil.Abstract;
using Soenneker.Jira.OpenApiClient;
using Soenneker.Utils.AsyncSingleton;

namespace Soenneker.Jira.OpenApiClientUtil;

public sealed class JiraOpenApiClientUtil : IJiraOpenApiClientUtil
{
    private readonly AsyncSingleton<JiraOpenApiClient> _client;

    public JiraOpenApiClientUtil(IJiraOpenApiHttpClient httpClientUtil, IConfiguration configuration)
    {
        _client = new AsyncSingleton<JiraOpenApiClient>(async token =>
        {
            HttpClient httpClient = await httpClientUtil.Get(token).NoSync();

            Uri baseAddress = httpClient.BaseAddress ?? throw new InvalidOperationException("The Jira HTTP client requires a base address.");
            var authenticationProvider = new JiraAuthenticationProvider(configuration, baseAddress);
            var requestAdapter = new HttpClientRequestAdapter(authenticationProvider, httpClient: httpClient)
            {
                BaseUrl = baseAddress.AbsoluteUri.TrimEnd('/')
            };
            return new JiraOpenApiClient(requestAdapter);
        });
    }

    public ValueTask<JiraOpenApiClient> Get(CancellationToken cancellationToken = default)
    {
        return _client.Get(cancellationToken);
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return _client.DisposeAsync();
    }
}

