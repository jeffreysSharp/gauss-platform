using AwesomeAssertions;
using Gauss.BuildingBlocks.Application.Abstractions.Results;

namespace Gauss.BuildingBlocks.UnitTests.Application.Results;

public sealed class ErrorTests
{
    [Fact(DisplayName = "Should create failure error")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Results")]
    public void Should_Create_Failure_Error()
    {
        // Arrange
        const string code = "Common.Failure";
        const string description = "A failure occurred.";

        // Act
        var error = Error.Failure(code, description);

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().Be(description);
        error.Type.Should().Be(ErrorType.Failure);
    }

    [Fact(DisplayName = "Should create validation error")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Results")]
    public void Should_Create_Validation_Error()
    {
        // Arrange
        const string code = "Common.Validation";
        const string description = "A validation error occurred.";

        // Act
        var error = Error.Validation(code, description);

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().Be(description);
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Should create not found error")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Results")]
    public void Should_Create_NotFound_Error()
    {
        // Arrange
        const string code = "Common.NotFound";
        const string description = "Resource was not found.";

        // Act
        var error = Error.NotFound(code, description);

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().Be(description);
        error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact(DisplayName = "Should create conflict error")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Results")]
    public void Should_Create_Conflict_Error()
    {
        // Arrange
        const string code = "Common.Conflict";
        const string description = "A conflict occurred.";

        // Act
        var error = Error.Conflict(code, description);

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().Be(description);
        error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact(DisplayName = "Should create unauthorized error")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Results")]
    public void Should_Create_Unauthorized_Error()
    {
        // Arrange
        const string code = "Common.Unauthorized";
        const string description = "Unauthorized access.";

        // Act
        var error = Error.Unauthorized(code, description);

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().Be(description);
        error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact(DisplayName = "Should create forbidden error")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Results")]
    public void Should_Create_Forbidden_Error()
    {
        // Arrange
        const string code = "Common.Forbidden";
        const string description = "Forbidden access.";

        // Act
        var error = Error.Forbidden(code, description);

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().Be(description);
        error.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact(DisplayName = "Should expose none error")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Results")]
    public void Should_Expose_None_Error()
    {
        // Act
        var error = Error.None;

        // Assert
        error.Code.Should().BeEmpty();
        error.Description.Should().BeEmpty();
        error.Type.Should().Be(ErrorType.Failure);
    }
}
