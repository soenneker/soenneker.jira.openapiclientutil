using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Jira.HttpClients.Abstract;

namespace Soenneker.Jira.OpenApiClientUtil.Tests;

internal sealed class TestHttpClient(HttpClient client) : IJiraOpenApiHttpClient
{
    public ValueTask<HttpClient> Get(CancellationToken cancellationToken = default) => ValueTask.FromResult(client);
    public void Dispose() { }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
