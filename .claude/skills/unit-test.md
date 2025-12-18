# Unit Test Pattern

Location: `tests/UserManagement.Tests.Unit/`

```csharp
using FluentAssertions;
using Moq;
using UserManagement.Core.Interfaces;

public class SubjectTests
{
    private readonly Mock<IDependency> _mockDependency;
    private readonly SubjectUnderTest _sut;

    public SubjectTests()
    {
        _mockDependency = new Mock<IDependency>();
        _sut = new SubjectUnderTest(_mockDependency.Object);
    }

    [Fact]
    public async Task MethodName_Scenario_ExpectedBehavior()
    {
        // Arrange
        _mockDependency.Setup(x => x.Method(It.IsAny<Type>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedValue);

        // Act
        var result = await _sut.MethodAsync(input);

        // Assert
        result.Should().NotBeNull();
        result.Property.Should().Be(expected);
        _mockDependency.Verify(x => x.Method(It.IsAny<Type>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

Conventions:
- Test class: `{ClassUnderTest}Tests`
- Method: `{Method}_{Scenario}_{ExpectedBehavior}`
- Use `It.IsAny<CancellationToken>()` for cancellation tokens
- FluentAssertions: `.Should().Be()`, `.Should().NotBeNull()`, `.Should().ThrowAsync<>()`
