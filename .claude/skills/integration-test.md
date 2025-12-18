# Integration Test Pattern

Location: `tests/UserManagement.Tests.Integration/ApiTests/`

```csharp
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using UserManagement.Application.DTOs;

public class ControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var createUserDto = new CreateUserDto(
            $"authuser_{Guid.NewGuid():N}",
            $"auth_{Guid.NewGuid():N}@example.com",
            "Password123!",
            "Auth",
            "User"
        );
        await _client.PostAsJsonAsync("/api/users", createUserDto);

        var loginDto = new LoginDto(createUserDto.Username, createUserDto.Password);
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResultDto>();
        return authResult!.Token;
    }

    [Fact]
    public async Task Endpoint_Scenario_ExpectedStatusCode()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/endpoint");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

Cover: success (200/201/204), validation (400), not found (404), unauthorized (401)
