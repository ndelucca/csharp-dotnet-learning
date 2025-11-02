using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using UserManagement.Application.DTOs;

namespace UserManagement.Tests.Integration.ApiTests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        // Arrange - First create a user
        var createUserDto = new CreateUserDto(
            "testuser",
            "test@example.com",
            "Password123!",
            "Test",
            "User"
        );

        await _client.PostAsJsonAsync("/api/users", createUserDto);

        var loginDto = new LoginDto("testuser", "Password123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.User.Username.Should().Be("testuser");
        result.User.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Login_InvalidUsername_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto("nonexistent", "Password123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange - Create a user first
        var createUserDto = new CreateUserDto(
            "testuser2",
            "test2@example.com",
            "Password123!",
            "Test",
            "User"
        );

        await _client.PostAsJsonAsync("/api/users", createUserDto);

        var loginDto = new LoginDto("testuser2", "WrongPassword!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
