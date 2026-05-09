using System.Text.Json;
using AwesomeAssertions;
using Gauss.BuildingBlocks.Api.Responses;
using Gauss.BuildingBlocks.Application.Abstractions.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http.Json;

namespace Gauss.BuildingBlocks.UnitTests.Api.Responses;

public sealed class ResultExtensionsTests
{

    [Fact(DisplayName = "Should return no content when result is successful")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Responses")]
    public async Task Should_Return_NoContent_When_Result_Is_Successful()
    {
        // Arrange
        var result = Result.Success();
        var httpResult = result.ToHttpResult();
        var httpContext = CreateHttpContext();

        // Act
        await httpResult.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact(DisplayName = "Should return ok when generic result is successful")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Responses")]
    public async Task Should_Return_Ok_When_Generic_Result_Is_Successful()
    {
        // Arrange
        var result = Result<TestResponse>.Success(new TestResponse("GAUSS"));
        var httpResult = result.ToHttpResult();
        var httpContext = CreateHttpContext();

        // Act
        await httpResult.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);

        var response = await JsonSerializer.DeserializeAsync<TestResponse>(
            httpContext.Response.Body,
            JsonSerializerOptions.Web);

        response.Should().NotBeNull();
        response!.Name.Should().Be("GAUSS");
    }

    [Fact(DisplayName = "Should return problem details when result is failure")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Responses")]
    public async Task Should_Return_ProblemDetails_When_Result_Is_Failure()
    {
        // Arrange
        var error = Error.Conflict(
            "Identity.User.EmailAlreadyExists",
            "A user with the specified email already exists.");

        var result = Result.Failure(error);
        var httpResult = result.ToHttpResult();
        var httpContext = CreateHttpContext();

        // Act
        await httpResult.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);

        using var jsonDocument = await JsonDocument.ParseAsync(httpContext.Response.Body);

        var root = jsonDocument.RootElement;

        root.GetProperty("status").GetInt32().Should().Be(StatusCodes.Status409Conflict);
        root.GetProperty("title").GetString().Should().Be("Conflict");
        root.GetProperty("detail").GetString().Should().Be("A user with the specified email already exists.");
        root.GetProperty("type").GetString().Should().Be("about:blank");
        root.GetProperty("code").GetString().Should().Be("Identity.User.EmailAlreadyExists");
    }

    [Fact(DisplayName = "Should use custom success mapping when generic result is successful")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Responses")]
    public async Task Should_Use_Custom_Success_Mapping_When_Generic_Result_Is_Successful()
    {
        // Arrange
        var result = Result<TestResponse>.Success(new TestResponse("GAUSS"));

        var httpResult = result.ToHttpResult(response =>
            Results.Created($"/resources/{response.Name}", response));

        var httpContext = CreateHttpContext();

        // Act
        await httpResult.ExecuteAsync(httpContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        httpContext.Response.Headers.Location.ToString().Should().Be("/resources/GAUSS");
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddOptions();

        services.Configure<JsonOptions>(_ =>
        {
        });

        var serviceProvider = services.BuildServiceProvider();

        return new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            Response =
        {
            Body = new MemoryStream()
        }
        };
    }

    private sealed record TestResponse(string Name);
}
