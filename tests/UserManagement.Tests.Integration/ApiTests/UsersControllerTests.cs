using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using UserManagement.Application.DTOs;

namespace UserManagement.Tests.Integration.ApiTests;

public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UsersControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        // Create a user
        var createUserDto = new CreateUserDto(
            $"authuser_{Guid.NewGuid():N}",
            $"auth_{Guid.NewGuid():N}@example.com",
            "Password123!",
            "Auth",
            "User"
        );

        await _client.PostAsJsonAsync("/api/users", createUserDto);

        // Login
        var loginDto = new LoginDto(createUserDto.Username, createUserDto.Password);
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResultDto>();

        return authResult!.Token;
    }

    [Fact]
    public async Task CreateUser_ValidData_ReturnsCreatedUser()
    {
        // Arrange
        var createUserDto = new CreateUserDto(
            $"newuser_{Guid.NewGuid():N}",
            $"new_{Guid.NewGuid():N}@example.com",
            "Password123!",
            "New",
            "User"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", createUserDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.Username.Should().Be(createUserDto.Username);
        user.Email.Should().Be(createUserDto.Email);
        user.FirstName.Should().Be(createUserDto.FirstName);
        user.LastName.Should().Be(createUserDto.LastName);
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateUser_DuplicateUsername_ReturnsBadRequest()
    {
        // Arrange
        var username = $"duplicate_{Guid.NewGuid():N}";
        var createUserDto1 = new CreateUserDto(
            username,
            $"user1_{Guid.NewGuid():N}@example.com",
            "Password123!",
            "First",
            "User"
        );

        var createUserDto2 = new CreateUserDto(
            username,
            $"user2_{Guid.NewGuid():N}@example.com",
            "Password123!",
            "Second",
            "User"
        );

        // Act
        await _client.PostAsJsonAsync("/api/users", createUserDto1);
        var response = await _client.PostAsJsonAsync("/api/users", createUserDto2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllUsers_WithAuthentication_ReturnsUsers()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAllUsers_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserById_ExistingUser_ReturnsUser()
    {
        // Arrange
        var createUserDto = new CreateUserDto(
            $"getuser_{Guid.NewGuid():N}",
            $"get_{Guid.NewGuid():N}@example.com",
            "Password123!",
            "Get",
            "User"
        );

        var createResponse = await _client.PostAsJsonAsync("/api/users", createUserDto);
        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>();

        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/api/users/{createdUser!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.Id.Should().Be(createdUser.Id);
        user.Username.Should().Be(createUserDto.Username);
    }

    [Fact]
    public async Task GetUserById_NonExistingUser_ReturnsNotFound()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/users/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUser_ValidData_ReturnsUpdatedUser()
    {
        // Arrange
        var createUserDto = new CreateUserDto(
            $"updateuser_{Guid.NewGuid():N}",
            $"update_{Guid.NewGuid():N}@example.com",
            "Password123!",
            "Update",
            "User"
        );

        var createResponse = await _client.PostAsJsonAsync("/api/users", createUserDto);
        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>();

        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateUserDto = new UpdateUserDto(
            $"updated_{Guid.NewGuid():N}@example.com",
            "Updated",
            "Name",
            true
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/users/{createdUser!.Id}", updateUserDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedUser = await response.Content.ReadFromJsonAsync<UserDto>();
        updatedUser.Should().NotBeNull();
        updatedUser!.Email.Should().Be(updateUserDto.Email);
        updatedUser.FirstName.Should().Be(updateUserDto.FirstName);
        updatedUser.LastName.Should().Be(updateUserDto.LastName);
        updatedUser.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteUser_ExistingUser_ReturnsNoContent()
    {
        // Arrange
        var createUserDto = new CreateUserDto(
            $"deleteuser_{Guid.NewGuid():N}",
            $"delete_{Guid.NewGuid():N}@example.com",
            "Password123!",
            "Delete",
            "User"
        );

        var createResponse = await _client.PostAsJsonAsync("/api/users", createUserDto);
        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>();

        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.DeleteAsync($"/api/users/{createdUser!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify user is deleted
        var getResponse = await _client.GetAsync($"/api/users/{createdUser.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_NonExistingUser_ReturnsNotFound()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/users/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
