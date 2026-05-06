using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Dapper;
using Gauss.Identity.ApiTests.Fixtures;
using Gauss.Identity.Domain.Users;
using Gauss.Testing.Api;
using Microsoft.Data.SqlClient;

namespace Gauss.Identity.ApiTests.Endpoints.Authentication;

public sealed class RefreshTokenEndpointTests(
    SqlServerTestDatabaseFixture databaseFixture)
    : IClassFixture<SqlServerTestDatabaseFixture>
{
    [Fact(DisplayName = "Should return OK when refresh token is valid")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Endpoints")]
    public async Task Should_Return_Ok_When_RefreshToken_Is_Valid()
    {
        // Arrange
        await using var factory = new IdentityApiFactory(databaseFixture);
        using var client = factory.CreateClient();

        var email = $"jeferson-{Guid.NewGuid():N}@gauss.com";
        const string password = "StrongPassword@123";

        await RegisterUserAsync(client, email, password);
        await ActivateUserAsync(email);

        var loginResponse = await LoginAsync(
            client,
            email,
            password);

        var request = new
        {
            RefreshToken = loginResponse.RefreshToken
        };

        // Act
        using var response = await client.PostAsJsonAsync(
            "/api/v1/identity/refresh-token",
            request);

        // Assert
        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.OK);

        response.ShouldHaveCorrelationId();

        await response.ShouldNotExposeSensitiveAuthenticationDataAsync();

        var root = await response.ReadJsonRootElementAsync();

        root.GetProperty("accessToken").GetString().Should().NotBeNullOrWhiteSpace();
        root.GetProperty("tokenType").GetString().Should().Be("Bearer");
        root.GetProperty("expiresAtUtc").GetDateTimeOffset().Should().BeAfter(DateTimeOffset.UtcNow);

        root.GetProperty("refreshToken").GetString().Should().NotBeNullOrWhiteSpace();
        root.GetProperty("refreshTokenExpiresAtUtc").GetDateTimeOffset().Should().BeAfter(DateTimeOffset.UtcNow);

        root.GetProperty("refreshToken").GetString().Should().NotBe(loginResponse.RefreshToken);
    }

    [Fact(DisplayName = "Should return unauthorized when refresh token is invalid")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Endpoints")]
    public async Task Should_Return_Unauthorized_When_RefreshToken_Is_Invalid()
    {
        // Arrange
        await using var factory = new IdentityApiFactory(databaseFixture);
        using var client = factory.CreateClient();

        var request = new
        {
            RefreshToken = "invalid-refresh-token"
        };

        // Act
        using var response = await client.PostAsJsonAsync(
            "/api/v1/identity/refresh-token",
            request);

        // Assert
        await response.ShouldBeProblemDetailsAsync(
            HttpStatusCode.Unauthorized,
            "Unauthorized",
            "Identity.RefreshToken.InvalidToken");
    }

    [Fact(DisplayName = "Should return bad request when refresh token is empty")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Endpoints")]
    public async Task Should_Return_Bad_Request_When_RefreshToken_Is_Empty()
    {
        // Arrange
        await using var factory = new IdentityApiFactory(databaseFixture);
        using var client = factory.CreateClient();

        var request = new
        {
            RefreshToken = string.Empty
        };

        // Act
        using var response = await client.PostAsJsonAsync(
            "/api/v1/identity/refresh-token",
            request);

        // Assert
        await response.ShouldBeProblemDetailsAsync(
            HttpStatusCode.BadRequest,
            "Validation Error",
            "Identity.RefreshToken.Required",
            "Refresh Token");
    }

    [Fact(DisplayName = "Should reject old refresh token after successful refresh")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Endpoints")]
    public async Task Should_Reject_Old_RefreshToken_After_Successful_Refresh()
    {
        // Arrange
        await using var factory = new IdentityApiFactory(databaseFixture);
        using var client = factory.CreateClient();

        var email = $"jeferson-{Guid.NewGuid():N}@gauss.com";
        const string password = "StrongPassword@123";

        await RegisterUserAsync(client, email, password);
        await ActivateUserAsync(email);

        var loginResponse = await LoginAsync(
            client,
            email,
            password);

        var refreshRequest = new
        {
            RefreshToken = loginResponse.RefreshToken
        };

        using var firstRefreshResponse = await client.PostAsJsonAsync(
            "/api/v1/identity/refresh-token",
            refreshRequest);

        await firstRefreshResponse.ShouldHaveStatusCodeAsync(HttpStatusCode.OK);

        // Act
        using var secondRefreshResponse = await client.PostAsJsonAsync(
            "/api/v1/identity/refresh-token",
            refreshRequest);

        // Assert
        await secondRefreshResponse.ShouldBeProblemDetailsAsync(
            HttpStatusCode.Unauthorized,
            "Unauthorized",
            "Identity.RefreshToken.InvalidToken");
    }

    private static async Task RegisterUserAsync(
        HttpClient client,
        string email,
        string password)
    {
        var request = new
        {
            Name = "Jeferson Almeida",
            Email = email,
            Password = password
        };

        using var response = await client.PostAsJsonAsync(
            "/api/v1/identity/register",
            request);

        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.Created);
    }

    private async Task ActivateUserAsync(string email)
    {
        await using var connection = new SqlConnection(databaseFixture.ConnectionString);

        const string sql = """
            UPDATE [identity].[Users]
            SET
                [Status] = @Status,
                [EmailConfirmedAtUtc] = @EmailConfirmedAtUtc
            WHERE [NormalizedEmail] = @NormalizedEmail
              AND [IsDeleted] = 0;
            """;

        var affectedRows = await connection.ExecuteAsync(
            sql,
            new
            {
                Status = (int)UserStatus.Active,
                EmailConfirmedAtUtc = new DateTimeOffset(2026, 04, 30, 12, 5, 0, TimeSpan.Zero),
                NormalizedEmail = email.ToLowerInvariant()
            });

        affectedRows.Should().Be(1);
    }

    private static async Task<LoginTokenResponse> LoginAsync(
        HttpClient client,
        string email,
        string password)
    {
        var request = new
        {
            Email = email,
            Password = password
        };

        using var response = await client.PostAsJsonAsync(
            "/api/v1/identity/login",
            request);

        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.OK);

        var root = await response.ReadJsonRootElementAsync();

        return new LoginTokenResponse(
            root.GetProperty("accessToken").GetString()
                ?? throw new InvalidOperationException("Access token was not returned."),
            root.GetProperty("refreshToken").GetString()
                ?? throw new InvalidOperationException("Refresh token was not returned."));
    }

    private sealed record LoginTokenResponse(
        string AccessToken,
        string RefreshToken);
}
