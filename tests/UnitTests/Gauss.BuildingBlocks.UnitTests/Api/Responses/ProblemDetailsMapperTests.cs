using AwesomeAssertions;
using Gauss.BuildingBlocks.Api.Responses;
using Gauss.BuildingBlocks.Application.Abstractions.Results;
using Microsoft.AspNetCore.Http;

namespace Gauss.BuildingBlocks.UnitTests.Api.Responses;

public sealed class ProblemDetailsMapperTests
{
    [Theory(DisplayName = "Should map error type to expected HTTP status code")]
    [Trait("Layer", "Api")]
    [Trait("Category", "ProblemDetails")]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.Unauthorized, StatusCodes.Status401Unauthorized)]
    [InlineData(ErrorType.Forbidden, StatusCodes.Status403Forbidden)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.Failure, StatusCodes.Status500InternalServerError)]
    public void Should_Map_ErrorType_To_Expected_Http_Status_Code(
        ErrorType errorType,
        int expectedStatusCode)
    {
        // Act
        var statusCode = ProblemDetailsMapper.GetStatusCode(errorType);

        // Assert
        statusCode.Should().Be(expectedStatusCode);
    }

    [Fact(DisplayName = "Should create problem details from error")]
    [Trait("Layer", "Api")]
    [Trait("Category", "ProblemDetails")]
    public void Should_Create_ProblemDetails_From_Error()
    {
        // Arrange
        var error = Error.Conflict(
            "Identity.User.EmailAlreadyExists",
            "A user with the specified email already exists.");

        // Act
        var problemDetails = ProblemDetailsMapper.ToProblemDetails(error);

        // Assert
        problemDetails.Status.Should().Be(StatusCodes.Status409Conflict);
        problemDetails.Title.Should().Be("Conflict");
        problemDetails.Detail.Should().Be("A user with the specified email already exists.");
        problemDetails.Type.Should().Be("about:blank");
        problemDetails.Extensions.Should().ContainKey("code");
        problemDetails.Extensions["code"].Should().Be("Identity.User.EmailAlreadyExists");
    }

    [Fact(DisplayName = "Should throw argument null exception when error is null")]
    [Trait("Layer", "Api")]
    [Trait("Category", "ProblemDetails")]
    public void Should_Throw_ArgumentNullException_When_Error_Is_Null()
    {
        // Act
        var action = () => ProblemDetailsMapper.ToProblemDetails(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }
}
