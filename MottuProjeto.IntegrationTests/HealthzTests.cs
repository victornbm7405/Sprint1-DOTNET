using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class HealthzTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthzTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GET_healthz_deve_retornar_200_e_status_Healthy()
    {
        var resp = await _client.GetAsync("/healthz");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var text = await resp.Content.ReadAsStringAsync();
        text.Should().Contain("Healthy");
    }

    [Fact]
    public async Task GET_healthz_ready_deve_retornar_200_e_status_Ready()
    {
        var resp = await _client.GetAsync("/healthz/ready");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var text = await resp.Content.ReadAsStringAsync();
        text.Should().Contain("Ready");
    }
}
