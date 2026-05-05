using System.Net;
using System.Text.Json;
using AwesomeAssertions;

namespace Gauss.Testing.Api;

public static class HttpResponseMessageExtensions
{
    public static async Task<string> ReadContentAsStringAsync(
        this HttpResponseMessage response)
    {
        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<JsonElement> ReadJsonRootElementAsync(
        this HttpResponseMessage response)
    {
        var content = await response.ReadContentAsStringAsync();

        using var jsonDocument = JsonDocument.Parse(content);

        return jsonDocument.RootElement.Clone();
    }

    public static async Task ShouldHaveStatusCodeAsync(
        this HttpResponseMessage response,
        HttpStatusCode expectedStatusCode)
    {
        var content = await response.ReadContentAsStringAsync();

        response.StatusCode.Should().Be(
            expectedStatusCode,
            because: content);
    }

    public static void ShouldHaveCorrelationId(
        this HttpResponseMessage response)
    {
        response.Headers.TryGetValues("X-Correlation-Id", out var correlationIds)
            .Should()
            .BeTrue();

        correlationIds.Should().NotBeNull();
        correlationIds!.Single().Should().NotBeNullOrWhiteSpace();
    }

    public static async Task ShouldNotExposeSensitiveAuthenticationDataAsync(
        this HttpResponseMessage response)
    {
        var content = await response.ReadContentAsStringAsync();

        content.Should().NotContain(
            "password",
            because: "the API must not expose plain text passwords");

        content.Should().NotContain(
            "passwordHash",
            because: "the API must not expose password hashes");
    }

    public static async Task ShouldBeProblemDetailsAsync(
        this HttpResponseMessage response,
        HttpStatusCode expectedStatusCode,
        string expectedTitle,
        string expectedCode)
    {
        await response.ShouldHaveStatusCodeAsync(expectedStatusCode);

        var root = await response.ReadJsonRootElementAsync();

        root.GetProperty("status").GetInt32().Should().Be((int)expectedStatusCode);
        root.GetProperty("title").GetString().Should().Be(expectedTitle);
        root.GetProperty("code").GetString().Should().Be(expectedCode);
    }

    public static async Task ShouldBeProblemDetailsAsync(
        this HttpResponseMessage response,
        HttpStatusCode expectedStatusCode,
        string expectedTitle,
        string expectedCode,
        string expectedDetailContains)
    {
        await response.ShouldBeProblemDetailsAsync(
            expectedStatusCode,
            expectedTitle,
            expectedCode);

        var root = await response.ReadJsonRootElementAsync();

        root.GetProperty("detail")
            .GetString()
            .Should()
            .Contain(expectedDetailContains);
    }
}
