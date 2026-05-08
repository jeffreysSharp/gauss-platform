using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AwesomeAssertions;
using Dapper;
using Gauss.Identity.ApiTests.Fixtures;
using Gauss.Identity.Domain.Users;
using Gauss.Testing.Api;
using Gauss.Testing.Fixtures;
using Microsoft.Data.SqlClient;

namespace Gauss.Identity.ApiTests.Endpoints.Authentication;

public sealed class CurrentUserEndpointTests(
    SqlServerTestDatabaseFixture databaseFixture)
    : IClassFixture<SqlServerTestDatabaseFixture>
{
    [Fact(DisplayName = "Should return OK when access token is valid")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Endpoints")]
    public async Task Should_Return_Ok_When_AccessToken_Is_Valid()
    {
        // Arrange
        await using var factory = new IdentityApiFactory(databaseFixture);
        using var client = factory.CreateClient();

        var email = $"jeferson-{Guid.NewGuid():N}@gauss.com";
        const string password = "StrongPassword@123";

        await RegisterUserAsync(client, email, password);
        await ActivateUserAsync(email);

        var accessToken = await LoginAndGetAccessTokenAsync(
            client,
            email,
            password);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            accessToken);

        // Act
        using var response = await client.GetAsync("/api/v1/identity/me");

        // Assert
        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.OK);

        response.ShouldHaveCorrelationId();

        await response.ShouldNotExposeSensitiveAuthenticationDataAsync();

        var root = await response.ReadJsonRootElementAsync();

        root.GetProperty("userId").GetGuid().Should().NotBe(Guid.Empty);
        root.GetProperty("tenantId").GetGuid().Should().NotBe(Guid.Empty);
        root.GetProperty("name").GetString().Should().Be("Jeferson Almeida");
        root.GetProperty("email").GetString().Should().Be(email);
    }

    [Fact(DisplayName = "Should return unauthorized when access token is missing")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Endpoints")]
    public async Task Should_Return_Unauthorized_When_AccessToken_Is_Missing()
    {
        // Arrange
        await using var factory = new IdentityApiFactory(databaseFixture);
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync("/api/v1/identity/me");

        // Assert
        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Should return unauthorized when access token is invalid")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Endpoints")]
    public async Task Should_Return_Unauthorized_When_AccessToken_Is_Invalid()
    {
        // Arrange
        await using var factory = new IdentityApiFactory(databaseFixture);
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            "invalid-access-token");

        // Act
        using var response = await client.GetAsync("/api/v1/identity/me");

        // Assert
        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.Unauthorized);
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

    private static async Task<string> LoginAndGetAccessTokenAsync(
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

        return root.GetProperty("accessToken").GetString()
            ?? throw new InvalidOperationException("Access token was not returned by login endpoint.");
    }
}
