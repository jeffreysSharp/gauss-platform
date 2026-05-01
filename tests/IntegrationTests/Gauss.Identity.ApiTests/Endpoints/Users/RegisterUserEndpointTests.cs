using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using Dapper;
using Gauss.Identity.ApiTests.Fixtures;
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

        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Created,
            because: content);

        response.Headers.Location.Should().NotBeNull();

        response.Headers.TryGetValues("X-Correlation-Id", out var correlationIds)
            .Should()
            .BeTrue(because: content);

        correlationIds.Should().NotBeNull();
        correlationIds!.Single().Should().NotBeNullOrWhiteSpace();

        content.Should().NotContain("password", because: "the API must not expose plain text passwords");
        content.Should().NotContain("passwordHash", because: "the API must not expose password hashes");

        using var jsonDocument = JsonDocument.Parse(content);
        var root = jsonDocument.RootElement;

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

        var firstContent = await firstResponse.Content.ReadAsStringAsync();

        firstResponse.StatusCode.Should().Be(
            HttpStatusCode.Created,
            because: firstContent);

        // Act
        using var response = await client.PostAsJsonAsync(
            "/api/v1/identity/register",
            request);

        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Conflict,
            because: content);

        using var jsonDocument = JsonDocument.Parse(content);
        var root = jsonDocument.RootElement;

        root.GetProperty("status").GetInt32().Should().Be(409);
        root.GetProperty("title").GetString().Should().Be("Conflict");
        root.GetProperty("code").GetString().Should().Be("Identity.User.EmailAlreadyExists");
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

        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var jsonDocument = JsonDocument.Parse(content);
        var root = jsonDocument.RootElement;

        root.GetProperty("status").GetInt32().Should().Be(400);
        root.GetProperty("title").GetString().Should().Be("Validation Error");
        root.GetProperty("code").GetString().Should().Be("Identity.User.NameRequired");
        root.GetProperty("detail").GetString().Should().Contain("Name");
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
            Email = "invalid-email",  // Invalid email format
            Password = "StrongPassword@123"
        };

        // Act
        using var response = await client.PostAsJsonAsync(
            "/api/v1/identity/register",
            request);

        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var jsonDocument = JsonDocument.Parse(content);
        var root = jsonDocument.RootElement;

        root.GetProperty("status").GetInt32().Should().Be(400);
        root.GetProperty("title").GetString().Should().Be("Validation Error");
        root.GetProperty("code").GetString().Should().Be("Identity.User.EmailInvalid");
        root.GetProperty("detail").GetString().Should().Contain("Email");
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
            Password = "weakpassword"  // Password without complexity (no special char, number, etc.)
        };

        // Act
        using var response = await client.PostAsJsonAsync(
            "/api/v1/identity/register",
            request);

        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var jsonDocument = JsonDocument.Parse(content);
        var root = jsonDocument.RootElement;

        root.GetProperty("status").GetInt32().Should().Be(400);
        root.GetProperty("title").GetString().Should().Be("Validation Error");
        root.GetProperty("code").GetString().Should().Be("Identity.User.PasswordRequiresUppercase");
        root.GetProperty("detail").GetString().Should().Contain("Password");
    }
}
