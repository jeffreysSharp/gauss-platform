using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using Dapper;
using Gauss.Identity.ApiTests.Fixtures;
using Gauss.Identity.Domain.Users;
using Gauss.Testing.Api;
using Microsoft.Data.SqlClient;

namespace Gauss.Identity.ApiTests.Endpoints.Authentication;

public sealed class LoginEndpointTests(
    SqlServerTestDatabaseFixture databaseFixture)
    : IClassFixture<SqlServerTestDatabaseFixture>
{
    [Fact(DisplayName = "Should return OK when login request is valid")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Endpoints")]
    public async Task Should_Return_Ok_When_Login_Request_Is_Valid()
    {
        // Arrange
        await using var factory = new IdentityApiFactory(databaseFixture);
        using var client = factory.CreateClient();

        var email = $"jeferson-{Guid.NewGuid():N}@gauss.com";
        const string password = "StrongPassword@123";

        await RegisterUserAsync(client, email, password);
        await ActivateUserAsync(email);

        var request = new
        {
            Email = email,
            Password = password
        };

        // Act
        using var response = await client.PostAsJsonAsync(
            "/api/v1/identity/login",
            request);

        // Assert
        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.OK);

        response.ShouldHaveCorrelationId();

        await response.ShouldNotExposeSensitiveAuthenticationDataAsync();

        var root = await response.ReadJsonRootElementAsync();

        root.GetProperty("userId").GetGuid().Should().NotBe(Guid.Empty);
        root.GetProperty("tenantId").GetGuid().Should().NotBe(Guid.Empty);
        root.GetProperty("name").GetString().Should().Be("Jeferson Almeida");
        root.GetProperty("email").GetString().Should().Be(email);
        root.GetProperty("accessToken").GetString().Should().NotBeNullOrWhiteSpace();
        root.GetProperty("tokenType").GetString().Should().Be("Bearer");
        root.GetProperty("expiresAtUtc").GetDateTimeOffset().Should().BeAfter(DateTimeOffset.UtcNow);

        await AssertLastLoginWasUpdatedAsync(email);
    }

    [Fact(DisplayName = "Should return unauthorized when password is invalid")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Endpoints")]
    public async Task Should_Return_Unauthorized_When_Password_Is_Invalid()
    {
        // Arrange
        await using var factory = new IdentityApiFactory(databaseFixture);
        using var client = factory.CreateClient();

        var email = $"jeferson-{Guid.NewGuid():N}@gauss.com";

        await RegisterUserAsync(client, email, "StrongPassword@123");
        await ActivateUserAsync(email);

        var request = new
        {
            Email = email,
            Password = "WrongPassword@123"
        };

        // Act
        using var response = await client.PostAsJsonAsync(
            "/api/v1/identity/login",
            request);

        // Assert
        await response.ShouldBeProblemDetailsAsync(
            HttpStatusCode.Unauthorized,
            "Unauthorized",
            "Identity.Login.InvalidCredentials");
    }

    [Fact(DisplayName = "Should return unauthorized when user does not exist")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Endpoints")]
    public async Task Should_Return_Unauthorized_When_User_Does_Not_Exist()
    {
        // Arrange
        await using var factory = new IdentityApiFactory(databaseFixture);
        using var client = factory.CreateClient();

        var request = new
        {
            Email = $"missing-{Guid.NewGuid():N}@gauss.com",
            Password = "StrongPassword@123"
        };

        // Act
        using var response = await client.PostAsJsonAsync(
            "/api/v1/identity/login",
            request);

        // Assert
        await response.ShouldBeProblemDetailsAsync(
            HttpStatusCode.Unauthorized,
            "Unauthorized",
            "Identity.Login.InvalidCredentials");
    }

    [Fact(DisplayName = "Should return forbidden when user is not active")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Endpoints")]
    public async Task Should_Return_Forbidden_When_User_Is_Not_Active()
    {
        // Arrange
        await using var factory = new IdentityApiFactory(databaseFixture);
        using var client = factory.CreateClient();

        var email = $"jeferson-{Guid.NewGuid():N}@gauss.com";
        const string password = "StrongPassword@123";

        await RegisterUserAsync(client, email, password);

        var request = new
        {
            Email = email,
            Password = password
        };

        // Act
        using var response = await client.PostAsJsonAsync(
            "/api/v1/identity/login",
            request);

        // Assert
        await response.ShouldBeProblemDetailsAsync(
            HttpStatusCode.Forbidden,
            "Forbidden",
            "Identity.Login.UserUnavailable");
    }

    [Fact(DisplayName = "Should return bad request when email is invalid")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Endpoints")]
    public async Task Should_Return_Bad_Request_When_Email_Is_Invalid()
    {
        // Arrange
        await using var factory = new IdentityApiFactory(databaseFixture);
        using var client = factory.CreateClient();

        var request = new
        {
            Email = "invalid-email",
            Password = "StrongPassword@123"
        };

        // Act
        using var response = await client.PostAsJsonAsync(
            "/api/v1/identity/login",
            request);

        // Assert
        await response.ShouldBeProblemDetailsAsync(
            HttpStatusCode.BadRequest,
            "Validation Error",
            "Identity.Login.EmailInvalid",
            "Email");
    }

    [Fact(DisplayName = "Should return bad request when password is empty")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Endpoints")]
    public async Task Should_Return_Bad_Request_When_Password_Is_Empty()
    {
        // Arrange
        await using var factory = new IdentityApiFactory(databaseFixture);
        using var client = factory.CreateClient();

        var request = new
        {
            Email = $"jeferson-{Guid.NewGuid():N}@gauss.com",
            Password = string.Empty
        };

        // Act
        using var response = await client.PostAsJsonAsync(
            "/api/v1/identity/login",
            request);

        // Assert
        await response.ShouldBeProblemDetailsAsync(
            HttpStatusCode.BadRequest,
            "Validation Error",
            "Identity.Login.PasswordRequired",
            "Password");
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

    private async Task AssertLastLoginWasUpdatedAsync(string email)
    {
        await using var connection = new SqlConnection(databaseFixture.ConnectionString);

        const string sql = """
            SELECT [LastLoginAtUtc]
            FROM [identity].[Users]
            WHERE [NormalizedEmail] = @NormalizedEmail
              AND [IsDeleted] = 0;
            """;

        var lastLoginAtUtc = await connection.QuerySingleAsync<DateTimeOffset?>(
            sql,
            new
            {
                NormalizedEmail = email.ToLowerInvariant()
            });

        lastLoginAtUtc.Should().NotBeNull();
    }
}
