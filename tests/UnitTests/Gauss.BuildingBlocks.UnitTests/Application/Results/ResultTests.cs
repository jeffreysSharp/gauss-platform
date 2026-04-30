using AwesomeAssertions;
using Gauss.BuildingBlocks.Application.Abstractions.Results;

namespace Gauss.BuildingBlocks.UnitTests.Application.Results;

public sealed class ResultTests
{
    [Fact(DisplayName = "Should create successful result")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Results")]
    public void Should_Create_Successful_Result()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact(DisplayName = "Should create failed result")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Results")]
    public void Should_Create_Failed_Result()
    {
        // Arrange
        var error = Error.Failure("Common.Failure", "A failure occurred.");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }
}
