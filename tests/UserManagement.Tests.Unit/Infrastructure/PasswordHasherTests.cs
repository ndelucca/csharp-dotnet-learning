using FluentAssertions;
using UserManagement.Infrastructure.Security;

namespace UserManagement.Tests.Unit.Infrastructure;

public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void HashPassword_ValidPassword_ReturnsNonEmptyHash()
    {
        // Arrange
        var password = "MySecurePassword123!";

        // Act
        var hash = _passwordHasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
    }

    [Fact]
    public void HashPassword_SamePassword_ReturnsDifferentHashes()
    {
        // Arrange
        var password = "MySecurePassword123!";

        // Act
        var hash1 = _passwordHasher.HashPassword(password);
        var hash2 = _passwordHasher.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2, "because each hash uses a unique salt");
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "MySecurePassword123!";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var password = "MySecurePassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_MultipleVerifications_ConsistentResults()
    {
        // Arrange
        var password = "MySecurePassword123!";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result1 = _passwordHasher.VerifyPassword(password, hash);
        var result2 = _passwordHasher.VerifyPassword(password, hash);
        var result3 = _passwordHasher.VerifyPassword(password, hash);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeTrue();
    }
}
