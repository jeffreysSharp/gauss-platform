using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AwesomeAssertions;
using Dapper;
using Gauss.Identity.ApiTests.Fixtures;
using Gauss.Identity.Application.Authorization;
using Gauss.Identity.Domain.Users;
using Gauss.Testing.Api;
using Gauss.Testing.Fixtures;
using Microsoft.Data.SqlClient;

namespace Gauss.Identity.ApiTests.Endpoints.Authorization;

public sealed class GetPermissionsEndpointTests(
    SqlServerTestDatabaseFixture databaseFixture)
    : IClassFixture<SqlServerTestDatabaseFixture>
{
    [Fact(DisplayName = "Should return OK when authenticated user has permissions read permission")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Endpoints")]
    public async Task Should_Return_Ok_When_Authenticated_User_Has_PermissionsRead_Permission()
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
        using var response = await client.GetAsync("/api/v1/identity/permissions");

        // Assert
        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.OK);

        response.ShouldHaveCorrelationId();

        await response.ShouldNotExposeSensitiveAuthenticationDataAsync();

        var root = await response.ReadJsonRootElementAsync();

        root.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Array);

        var permissionCodes = root
            .EnumerateArray()
            .Select(permission => permission.GetProperty("code").GetString())
            .ToList();

        permissionCodes.Should().Contain(IdentityPermissions.PermissionsRead);
        permissionCodes.Should().Contain(IdentityPermissions.UsersRead);
        permissionCodes.Should().Contain(IdentityPermissions.RolesRead);
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
        using var response = await client.GetAsync("/api/v1/identity/permissions");

        // Assert
        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Should return forbidden when authenticated user does not have permissions read permission")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Endpoints")]
    public async Task Should_Return_Forbidden_When_Authenticated_User_Does_Not_Have_PermissionsRead_Permission()
    {
        // Arrange
        await using var factory = new IdentityApiFactory(databaseFixture);
        using var client = factory.CreateClient();

        var email = $"jeferson-{Guid.NewGuid():N}@gauss.com";
        const string password = "StrongPassword@123";

        var registration = await RegisterUserAsync(client, email, password);

        await RemoveUserPermissionsAsync(
            registration.UserId,
            registration.TenantId);

        await ActivateUserAsync(email);

        var accessToken = await LoginAndGetAccessTokenAsync(
            client,
            email,
            password);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            accessToken);

        // Act
        using var response = await client.GetAsync("/api/v1/identity/permissions");

        // Assert
        await response.ShouldHaveStatusCodeAsync(HttpStatusCode.Forbidden);
    }

    private static async Task<RegisterUserResult> RegisterUserAsync(
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

        var root = await response.ReadJsonRootElementAsync();

        return new RegisterUserResult(
            root.GetProperty("userId").GetGuid(),
            root.GetProperty("tenantId").GetGuid());
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

    private async Task RemoveUserPermissionsAsync(
        Guid userId,
        Guid tenantId)
    {
        await using var connection = new SqlConnection(databaseFixture.ConnectionString);

        const string sql = """
            UPDATE rp
            SET rp.[IsDeleted] = 1
            FROM [identity].[RolePermissions] rp
            INNER JOIN [identity].[UserRoles] ur
                ON ur.[RoleId] = rp.[RoleId]
            WHERE ur.[UserId] = @UserId
              AND ur.[TenantId] = @TenantId
              AND ur.[IsDeleted] = 0
              AND rp.[IsDeleted] = 0;
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                UserId = userId,
                TenantId = tenantId
            });
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

    private sealed record RegisterUserResult(
        Guid UserId,
        Guid TenantId);
}
