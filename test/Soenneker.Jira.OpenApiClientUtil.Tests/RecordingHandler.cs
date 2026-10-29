using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Jira.OpenApiClientUtil.Tests;

internal sealed class RecordingHandler : HttpMessageHandler
{
    public List<string> Urls { get; } = [];
    public List<string?> Authorizations { get; } = [];
    public List<string?> Revisions { get; } = [];
    public List<string?> Accepts { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Urls.Add(request.RequestUri!.AbsoluteUri);
        Authorizations.Add(request.Headers.Authorization?.ToString());
        Revisions.Add(request.Headers.TryGetValues("revision", out var revisions) ? revisions.Single() : null);
        Accepts.Add(request.Headers.Accept.ToString());
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
        });
    }
}
