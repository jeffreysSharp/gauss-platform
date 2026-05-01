using AwesomeAssertions;
using Dapper;
using Gauss.Identity.Domain.Users;
using Gauss.Identity.Domain.Users.ValueObjects;
using Gauss.Identity.Infrastructure.Persistence;
using Gauss.Identity.InfrastructureTests.Fixtures;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Gauss.Identity.InfrastructureTests.Persistence;

public sealed class SqlUserRepositoryTests(
    SqlServerTestDatabaseFixture fixture)
    : IClassFixture<SqlServerTestDatabaseFixture>
{
    [Fact(DisplayName = "Should add user when user is valid")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Add_User_When_User_Is_Valid()
    {
        // Arrange
        var repository = CreateRepository();

        var user = CreateUser();

        // Act
        await repository.AddAsync(user);

        // Assert
        await using var connection = new SqlConnection(fixture.ConnectionString);

        const string sql = """
            SELECT
                [Id],
                [TenantId],
                [Name],
                [Email],
                [NormalizedEmail],
                [PasswordHash],
                [Status]
            FROM [identity].[Users]
            WHERE [Id] = @UserId;
            """;

        var persistedUser = await connection.QuerySingleAsync<dynamic>(
            sql,
            new
            {
                UserId = user.Id.Value
            });

        ((Guid)persistedUser.Id).Should().Be(user.Id.Value);
        ((Guid)persistedUser.TenantId).Should().Be(user.TenantId.Value);
        ((string)persistedUser.Name).Should().Be(user.Name);
        ((string)persistedUser.Email).Should().Be(user.Email.Value);
        ((string)persistedUser.NormalizedEmail).Should().Be(user.Email.Value);
        ((string)persistedUser.PasswordHash).Should().Be(user.PasswordHash.Value);
        ((int)persistedUser.Status).Should().Be((int)user.Status);
    }

    [Fact(DisplayName = "Should return true when email exists in same tenant")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Return_True_When_Email_Exists_In_Same_Tenant()
    {
        // Arrange
        var repository = CreateRepository();

        var user = CreateUser();

        await repository.AddAsync(user);

        // Act
        var exists = await repository.ExistsByEmailAsync(            
            user.Email);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact(DisplayName = "Should return true when email exists globally")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Return_True_When_Email_Exists_Globally()
    {
        // Arrange
        var repository = CreateRepository();

        var user = CreateUser();

        await repository.AddAsync(user);

        // Act
        var exists = await repository.ExistsByEmailAsync(user.Email);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact(DisplayName = "Should not persist plain text password")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Not_Persist_Plain_Text_Password()
    {
        // Arrange
        var repository = CreateRepository();

        var user = User.Register(
            TenantId.New(),
            "Jeferson Almeida",
            Email.Create("jeferson@gauss.com"),
            PasswordHash.Create("hashed-password-value"),
            new DateTimeOffset(2026, 04, 30, 12, 0, 0, TimeSpan.Zero));

        // Act
        await repository.AddAsync(user);

        // Assert
        await using var connection = new SqlConnection(fixture.ConnectionString);

        const string sql = """
            SELECT [PasswordHash]
            FROM [identity].[Users]
            WHERE [Id] = @UserId;
            """;

        var passwordHash = await connection.QuerySingleAsync<string>(
            sql,
            new
            {
                UserId = user.Id.Value
            });

        passwordHash.Should().NotBe("StrongPassword@123");
        passwordHash.Should().Be("hashed-password-value");
    }

    private SqlUserRepository CreateRepository()
    {
        var options = Options.Create(new IdentityPersistenceOptions
        {
            ConnectionString = fixture.ConnectionString
        });

        var connectionFactory = new IdentityDbConnectionFactory(options);

        return new SqlUserRepository(connectionFactory);
    }

    private static User CreateUser()
    {
        return User.Register(
            TenantId.New(),
            "Jeferson Almeida",
            Email.Create($"jeferson-{Guid.NewGuid():N}@gauss.com"),
            PasswordHash.Create("hashed-password-value"),
            new DateTimeOffset(2026, 04, 30, 12, 0, 0, TimeSpan.Zero));
    }
}
