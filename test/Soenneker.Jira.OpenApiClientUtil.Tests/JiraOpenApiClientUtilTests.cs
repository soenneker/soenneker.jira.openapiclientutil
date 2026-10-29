using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Soenneker.Jira.OpenApiClientUtil.Tests;

public sealed class JiraOpenApiClientUtilTests
{
    [Test]
    public async Task UsesConfiguredBaseUrlAndCurrentCredentials()
    {
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jira:Email"] = "user@example.com", ["Jira:ApiToken"] = "first"
        }).Build();
        using var handler = new RecordingHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://tenant.atlassian.net/") };
        await using var utility = new JiraOpenApiClientUtil(new TestHttpClient(httpClient), configuration);
        var client = await utility.Get();
        if (!ReferenceEquals(client, await utility.Get()))
            throw new Exception("The client was not cached.");

        await client.Rest.Api.Three.Myself.GetAsync();
        configuration["Jira:ApiToken"] = "refreshed";
        await client.Rest.Api.Three.Myself.GetAsync();

        if (handler.Urls.Count != 2 || handler.Urls[0] != "https://tenant.atlassian.net/rest/api/3/myself" ||
            handler.Authorizations[0] != "Basic dXNlckBleGFtcGxlLmNvbTpmaXJzdA==" || handler.Authorizations[1] != "Basic dXNlckBleGFtcGxlLmNvbTpyZWZyZXNoZWQ=")
            throw new Exception("The https://tenant.atlassian.net/rest/api/3/myself or credentials were incorrect.");
        
    }

    [Test]
    public async Task RejectsUntrustedHosts()
    {
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Jira:Email"] = "user@example.com", ["Jira:ApiToken"] = "first" }).Build();
        using var handler = new RecordingHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://tenant.atlassian.net/") };
        await using var utility = new JiraOpenApiClientUtil(new TestHttpClient(httpClient), configuration);
        var client = await utility.Get();
        try { await client.Rest.Api.Three.Myself.WithUrl("https://other.example/resource").GetAsync(); }
        catch (InvalidOperationException)
        {
            if (handler.Urls.Count != 0) throw new Exception("Credentials escaped to another host.");
            return;
        }
        throw new Exception("An untrusted host was accepted.");
    }

    [Test]
    public async Task RejectsMissingCredentialsBeforeSending()
    {
        IConfiguration configuration = new ConfigurationBuilder().Build();
        using var handler = new RecordingHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://tenant.atlassian.net/") };
        await using var utility = new JiraOpenApiClientUtil(new TestHttpClient(httpClient), configuration);
        var client = await utility.Get();
        try { await client.Rest.Api.Three.Myself.GetAsync(); }
        catch (InvalidOperationException)
        {
            if (handler.Urls.Count != 0) throw new Exception("A request was sent without credentials.");
            return;
        }
        throw new Exception("Missing credentials were accepted.");
    }
    [Test]
    public async Task UsesBearerTokenAndCloudIdBasePath()
    {
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jira:AuthMode"] = "Bearer",
            ["Jira:AccessToken"] = "oauth-token"
        }).Build();
        using var handler = new RecordingHandler();
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.atlassian.com/ex/jira/cloud-id/") };
        await using var utility = new JiraOpenApiClientUtil(new TestHttpClient(httpClient), configuration);
        var client = await utility.Get();
        await client.Rest.Api.Three.Myself.GetAsync();
        if (handler.Urls[0] != "https://api.atlassian.com/ex/jira/cloud-id/rest/api/3/myself" ||
            handler.Authorizations[0] != "Bearer oauth-token")
            throw new Exception("The OAuth URL or bearer token was incorrect.");

        try
        {
            await client.Rest.Api.Three.Myself.WithUrl("https://api.atlassian.com/ex/jira/another-cloud/rest/api/3/myself").GetAsync();
        }
        catch (InvalidOperationException)
        {
            if (handler.Urls.Count != 1)
                throw new Exception("Credentials escaped to another cloud.");
            return;
        }
        throw new Exception("A different cloud ID was accepted.");
    }}


