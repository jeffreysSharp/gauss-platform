using AwesomeAssertions;
using Gauss.BuildingBlocks.Application.Abstractions.Results;

namespace Gauss.BuildingBlocks.UnitTests.Application.Results;

public sealed class ResultOfTTests
{
    [Fact(DisplayName = "Should create successful result with value")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Results")]
    public void Should_Create_Successful_Result_With_Value()
    {
        // Arrange
        const string value = "GAUSS";

        // Act
        var result = Result<string>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
        result.Value.Should().Be(value);
    }

    [Fact(DisplayName = "Should create failed result with error")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Results")]
    public void Should_Create_Failed_Result_With_Error()
    {
        // Arrange
        var error = Error.Validation("Common.Validation", "A validation error occurred.");

        // Act
        var result = Result<string>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact(DisplayName = "Should throw invalid operation exception when accessing value from failed result")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Results")]
    public void Should_Throw_InvalidOperationException_When_Accessing_Value_From_Failed_Result()
    {
        // Arrange
        var error = Error.Failure("Common.Failure", "A failure occurred.");
        var result = Result<string>.Failure(error);

        // Act
        var action = () => result.Value;

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }
}
