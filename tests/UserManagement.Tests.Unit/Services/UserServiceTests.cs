using FluentAssertions;
using Moq;
using UserManagement.Application.DTOs;
using UserManagement.Application.Services;
using UserManagement.Core.Entities;
using UserManagement.Core.Interfaces;

namespace UserManagement.Tests.Unit.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _userService = new UserService(_mockRepository.Object, _mockPasswordHasher.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _mockRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Username.Should().Be("testuser");
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingUser_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidUser_ReturnsCreatedUser()
    {
        // Arrange
        var createDto = new CreateUserDto(
            "newuser",
            "new@example.com",
            "password123",
            "New",
            "User"
        );

        _mockRepository.Setup(r => r.GetByUsernameAsync(createDto.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _mockRepository.Setup(r => r.GetByEmailAsync(createDto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _mockPasswordHasher.Setup(p => p.HashPassword(createDto.Password))
            .Returns("hashed_password");
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken ct) => u);

        // Act
        var result = await _userService.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(createDto.Username);
        result.Email.Should().Be(createDto.Email);
        result.FirstName.Should().Be(createDto.FirstName);
        result.LastName.Should().Be(createDto.LastName);
        result.IsActive.Should().BeTrue();

        _mockPasswordHasher.Verify(p => p.HashPassword(createDto.Password), Times.Once);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateUsername_ThrowsException()
    {
        // Arrange
        var createDto = new CreateUserDto(
            "existinguser",
            "new@example.com",
            "password123",
            "New",
            "User"
        );

        var existingUser = new User { Id = Guid.NewGuid(), Username = "existinguser" };
        _mockRepository.Setup(r => r.GetByUsernameAsync(createDto.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var act = async () => await _userService.CreateAsync(createDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Username*already exists*");
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var createDto = new CreateUserDto(
            "newuser",
            "existing@example.com",
            "password123",
            "New",
            "User"
        );

        _mockRepository.Setup(r => r.GetByUsernameAsync(createDto.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var existingUser = new User { Id = Guid.NewGuid(), Email = "existing@example.com" };
        _mockRepository.Setup(r => r.GetByEmailAsync(createDto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var act = async () => await _userService.CreateAsync(createDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Email*already exists*");
    }

    [Fact]
    public async Task UpdateAsync_ExistingUser_ReturnsUpdatedUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "old@example.com",
            FirstName = "Old",
            LastName = "Name",
            IsActive = true
        };

        var updateDto = new UpdateUserDto(
            "new@example.com",
            "New",
            "Name",
            true
        );

        _mockRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);
        _mockRepository.Setup(r => r.GetByEmailAsync(updateDto.Email!, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken ct) => u);

        // Act
        var result = await _userService.UpdateAsync(userId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(updateDto.Email);
        result.FirstName.Should().Be(updateDto.FirstName);
        result.LastName.Should().Be(updateDto.LastName);
        result.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_NonExistingUser_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = new UpdateUserDto("new@example.com", "New", "Name", true);

        _mockRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _userService.UpdateAsync(userId, updateDto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ExistingUser_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.DeleteAsync(userId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingUser_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _userService.DeleteAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Username = "user1", Email = "user1@example.com" },
            new User { Id = Guid.NewGuid(), Username = "user2", Email = "user2@example.com" }
        };

        _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _userService.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Username == "user1");
        result.Should().Contain(u => u.Username == "user2");
    }
}
