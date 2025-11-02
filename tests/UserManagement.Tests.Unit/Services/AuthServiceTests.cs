using FluentAssertions;
using Moq;
using UserManagement.Application.DTOs;
using UserManagement.Application.Services;
using UserManagement.Core.Entities;
using UserManagement.Core.Interfaces;

namespace UserManagement.Tests.Unit.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockTokenService = new Mock<ITokenService>();
        _authService = new AuthService(
            _mockRepository.Object,
            _mockPasswordHasher.Object,
            _mockTokenService.Object
        );
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResult()
    {
        // Arrange
        var loginDto = new LoginDto("testuser", "password123");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            FirstName = "Test",
            LastName = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.GetByUsernameAsync(loginDto.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockPasswordHasher.Setup(p => p.VerifyPassword(loginDto.Password, user.PasswordHash))
            .Returns(true);
        _mockTokenService.Setup(t => t.GenerateToken(user))
            .Returns("jwt_token");

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be("jwt_token");
        result.User.Username.Should().Be("testuser");
        result.User.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task LoginAsync_InvalidUsername_ReturnsNull()
    {
        // Arrange
        var loginDto = new LoginDto("nonexistent", "password123");
        _mockRepository.Setup(r => r.GetByUsernameAsync(loginDto.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().BeNull();
        _mockPasswordHasher.Verify(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsNull()
    {
        // Arrange
        var loginDto = new LoginDto("testuser", "wrongpassword");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            PasswordHash = "hashed_password",
            IsActive = true
        };

        _mockRepository.Setup(r => r.GetByUsernameAsync(loginDto.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockPasswordHasher.Setup(p => p.VerifyPassword(loginDto.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().BeNull();
        _mockTokenService.Verify(t => t.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ReturnsNull()
    {
        // Arrange
        var loginDto = new LoginDto("testuser", "password123");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            PasswordHash = "hashed_password",
            IsActive = false
        };

        _mockRepository.Setup(r => r.GetByUsernameAsync(loginDto.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.Should().BeNull();
        _mockPasswordHasher.Verify(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
