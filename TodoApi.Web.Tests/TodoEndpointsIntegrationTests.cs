using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TodoApi.Web;
using TodoApi.Web.Features.Auth;
using TodoApi.Web.Features.Todos;
using Xunit;

namespace TodoApi.Web.Tests;

[CollectionDefinition("IntegrationTests", DisableParallelization = true)]
public class IntegrationTestsCollection { }

[Collection("IntegrationTests")]
public class TodoEndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TodoEndpointsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthCheck_Returns_Healthy_Status()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v2/health");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
    }

    [Fact]
    public async Task Full_Auth_And_Todo_Flow_Works()
    {
        var client = _factory.CreateClient();

        // Register unique user each test
        var username = $"testuser_{Guid.NewGuid():N}";
        var registerDto = new { Username = username, Password = "Password123!" };

        var regResp = await client.PostAsJsonAsync("/api/v2/auth/register", registerDto);
        regResp.EnsureSuccessStatusCode();

        // Login
        var loginResp = await client.PostAsJsonAsync("/api/v2/auth/login", registerDto);
        loginResp.EnsureSuccessStatusCode();

        var authResult = await loginResp.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        Assert.NotNull(authResult);
        Assert.NotNull(authResult.Data);
        Assert.False(string.IsNullOrEmpty(authResult.Data.AccessToken));

        var token = authResult.Data.AccessToken;

        // Create Todo (protected endpoint)
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createDto = new { Title = "Integration Test Todo", DueDate = DateTimeOffset.UtcNow.AddDays(2) };
        var createResp = await client.PostAsJsonAsync("/api/v2/todos", createDto);
        createResp.EnsureSuccessStatusCode();

        var todoResult = await createResp.Content.ReadFromJsonAsync<ApiResponse<TodoResponseDto>>();
        Assert.NotNull(todoResult);
        Assert.NotNull(todoResult.Data);
        Assert.Equal("Integration Test Todo", todoResult.Data.Title);
    }
}
