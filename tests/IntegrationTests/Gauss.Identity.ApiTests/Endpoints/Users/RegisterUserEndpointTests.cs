using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using Dapper;
using Gauss.Identity.ApiTests.Fixtures;
using Gauss.Testing.Api;
using Microsoft.Data.SqlClient;

namespace Gauss.Identity.ApiTests.Endpoints.Users;

public sealed class RegisterUserEndpointTests(
    SqlServerTestDatabaseFixture databaseFixture)
    : IClassFixture<SqlServerTestDatabaseFixture>
{
    [Fact(DisplayName = "Should return created when register user request is valid")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Endpoints")]
    public async Task Should_Return_Created_When_RegisterUser_Request_Is_Valid()
    {
        // Arrange
        await using var factory = new IdentityApiFactory(databaseFixture);
        using var client = factory.CreateClient();

        var request = new
        {
            Name = "Jeferson Almeida",
            Email = $"jeferson-{Guid.NewGuid():N}@gauss.com",
            Password = "StrongPassword@123"
        };

        // Act
        using var response = await client.PostAsJsonAsync(
            "/api/v1/identity/register",
            request);

        // Assert
        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.Created);

        response.Headers.Location.Should().NotBeNull();

        response.ShouldHaveCorrelationId();

        await response.ShouldNotExposeSensitiveAuthenticationDataAsync();

        var root = await response.ReadJsonRootElementAsync();

        root.GetProperty("userId").GetGuid().Should().NotBe(Guid.Empty);
        root.GetProperty("tenantId").GetGuid().Should().NotBe(Guid.Empty);
        root.GetProperty("name").GetString().Should().Be(request.Name);
        root.GetProperty("email").GetString().Should().Be(request.Email);

        await AssertUserWasPersistedAsync(request.Email);
    }

    [Fact(DisplayName = "Should return conflict when email already exists")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Endpoints")]
    public async Task Should_Return_Conflict_When_Email_Already_Exists()
    {
        // Arrange
        await using var factory = new IdentityApiFactory(databaseFixture);
        using var client = factory.CreateClient();

        var email = $"duplicated-{Guid.NewGuid():N}@gauss.com";

        var request = new
        {
            Name = "Jeferson Almeida",
            Email = email,
            Password = "StrongPassword@123"
        };

        using var firstResponse = await client.PostAsJsonAsync(
            "/api/v1/identity/register",
            request);

        await firstResponse.ShouldHaveStatusCodeAsync(HttpStatusCode.Created);

        // Act
        using var response = await client.PostAsJsonAsync(
            "/api/v1/identity/register",
            request);

        // Assert
        await response.ShouldBeProblemDetailsAsync(
            HttpStatusCode.Conflict,
            "Conflict",
            "Identity.User.EmailAlreadyExists");
    }

    [Fact(DisplayName = "Should return bad request when name is invalid")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Endpoints")]
    public async Task Should_Return_Bad_Request_When_Name_Is_Invalid()
    {
        // Arrange
        await using var factory = new IdentityApiFactory(databaseFixture);
        using var client = factory.CreateClient();

        var request = new
        {
            Name = "",
            Email = $"jeferson-{Guid.NewGuid():N}@gauss.com",
            Password = "StrongPassword@123"
        };

        // Act
        using var response = await client.PostAsJsonAsync(
            "/api/v1/identity/register",
            request);

        // Assert
        await response.ShouldBeProblemDetailsAsync(
            HttpStatusCode.BadRequest,
            "Validation Error",
            "Identity.User.NameRequired",
            "Name");
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
            Name = "Jeferson Almeida",
            Email = "invalid-email",
            Password = "StrongPassword@123"
        };

        // Act
        using var response = await client.PostAsJsonAsync(
            "/api/v1/identity/register",
            request);

        // Assert
        await response.ShouldBeProblemDetailsAsync(
            HttpStatusCode.BadRequest,
            "Validation Error",
            "Identity.User.EmailInvalid",
            "Email");
    }

    [Fact(DisplayName = "Should return bad request when password is weak")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Endpoints")]
    public async Task Should_Return_Bad_Request_When_Password_Is_Weak()
    {
        // Arrange
        await using var factory = new IdentityApiFactory(databaseFixture);
        using var client = factory.CreateClient();

        var request = new
        {
            Name = "Jeferson Almeida",
            Email = $"jeferson-{Guid.NewGuid():N}@gauss.com",
            Password = "weakpassword"
        };

        // Act
        using var response = await client.PostAsJsonAsync(
            "/api/v1/identity/register",
            request);

        // Assert
        await response.ShouldBeProblemDetailsAsync(
            HttpStatusCode.BadRequest,
            "Validation Error",
            "Identity.User.PasswordRequiresUppercase",
            "Password");
    }

    private async Task AssertUserWasPersistedAsync(string email)
    {
        await using var connection = new SqlConnection(databaseFixture.ConnectionString);

        const string sql = """
            SELECT COUNT(1)
            FROM [identity].[Users]
            WHERE [NormalizedEmail] = @NormalizedEmail
              AND [IsDeleted] = 0;
            """;

        var count = await connection.ExecuteScalarAsync<int>(
            sql,
            new
            {
                NormalizedEmail = email.ToLowerInvariant()
            });

        count.Should().Be(1);
    }
}
