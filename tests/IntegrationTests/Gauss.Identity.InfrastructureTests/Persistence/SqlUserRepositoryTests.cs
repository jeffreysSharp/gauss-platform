using AwesomeAssertions;
using Dapper;
using Gauss.BuildingBlocks.Domain.Tenants;
using Gauss.Identity.Domain.Users;
using Gauss.Identity.Domain.Users.ValueObjects;
using Gauss.Identity.Infrastructure.Persistence;
using Gauss.Testing.Fixtures;
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

        await AddTenantAsync(user.TenantId);

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

        var persistedUser = await connection.QuerySingleAsync<UserRecord>(
            sql,
            new
            {
                UserId = user.Id.Value
            });

        persistedUser.Id.Should().Be(user.Id.Value);
        persistedUser.TenantId.Should().Be(user.TenantId.Value);
        persistedUser.Name.Should().Be(user.Name);
        persistedUser.Email.Should().Be(user.Email.Value);
        persistedUser.NormalizedEmail.Should().Be(user.Email.Value);
        persistedUser.PasswordHash.Should().Be(user.PasswordHash.Value);
        persistedUser.Status.Should().Be((int)user.Status);
    }

    [Fact(DisplayName = "Should not create tenant implicitly when adding user")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Not_Create_Tenant_Implicitly_When_Adding_User()
    {
        // Arrange
        var repository = CreateRepository();

        var user = CreateUser();

        // Act
        var act = async () => await repository.AddAsync(user);

        // Assert
        await act.Should().ThrowAsync<SqlException>();
    }

    [Fact(DisplayName = "Should return true when email exists globally")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Return_True_When_Email_Exists_Globally()
    {
        // Arrange
        var repository = CreateRepository();

        var user = CreateUser();

        await AddTenantAsync(user.TenantId);
        await repository.AddAsync(user);

        // Act
        var exists = await repository.ExistsByEmailAsync(user.Email);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact(DisplayName = "Should return false when email does not exist globally")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Return_False_When_Email_Does_Not_Exist_Globally()
    {
        // Arrange
        var repository = CreateRepository();

        var email = Email.Create($"missing-{Guid.NewGuid():N}@gauss.com");

        // Act
        var exists = await repository.ExistsByEmailAsync(email);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact(DisplayName = "Should return user when email exists")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Return_User_When_Email_Exists()
    {
        // Arrange
        var repository = CreateRepository();

        var user = CreateUser();

        user.ConfirmEmail(new DateTimeOffset(2026, 04, 30, 12, 5, 0, TimeSpan.Zero));

        await AddTenantAsync(user.TenantId);
        await repository.AddAsync(user);

        // Act
        var persistedUser = await repository.GetByEmailAsync(user.Email);

        // Assert
        persistedUser.Should().NotBeNull();

        persistedUser!.Id.Should().Be(user.Id);
        persistedUser.TenantId.Should().Be(user.TenantId);
        persistedUser.Name.Should().Be(user.Name);
        persistedUser.Email.Should().Be(user.Email);
        persistedUser.PasswordHash.Value.Should().Be(user.PasswordHash.Value);
        persistedUser.Status.Should().Be(user.Status);
        persistedUser.RegisteredAtUtc.Should().Be(user.RegisteredAtUtc);
        persistedUser.EmailConfirmedAtUtc.Should().Be(user.EmailConfirmedAtUtc);
        persistedUser.LastLoginAtUtc.Should().Be(user.LastLoginAtUtc);
        persistedUser.LockedUntilUtc.Should().Be(user.LockedUntilUtc);
    }

    [Fact(DisplayName = "Should return null when email does not exist")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Return_Null_When_Email_Does_Not_Exist()
    {
        // Arrange
        var repository = CreateRepository();

        var email = Email.Create($"missing-{Guid.NewGuid():N}@gauss.com");

        // Act
        var user = await repository.GetByEmailAsync(email);

        // Assert
        user.Should().BeNull();
    }

    [Fact(DisplayName = "Should record last login when user login succeeds")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Record_LastLoginAtUtc_When_User_Login_Succeeds()
    {
        // Arrange
        var repository = CreateRepository();

        var user = CreateUser();

        await AddTenantAsync(user.TenantId);
        await repository.AddAsync(user);

        var loggedInAtUtc = new DateTimeOffset(
            2026,
            04,
            30,
            13,
            0,
            0,
            TimeSpan.Zero);

        // Act
        await repository.RecordLoginAsync(
            user.Id,
            loggedInAtUtc);

        // Assert
        await using var connection = new SqlConnection(fixture.ConnectionString);

        const string sql = """
            SELECT [LastLoginAtUtc]
            FROM [identity].[Users]
            WHERE [Id] = @UserId
              AND [IsDeleted] = 0;
            """;

        var lastLoginAtUtc = await connection.QuerySingleAsync<DateTimeOffset?>(
            sql,
            new
            {
                UserId = user.Id.Value
            });

        lastLoginAtUtc.Should().Be(loggedInAtUtc);
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
            Email.Create($"jeferson-{Guid.NewGuid():N}@gauss.com"),
            PasswordHash.Create("hashed-password-value"),
            new DateTimeOffset(2026, 04, 30, 12, 0, 0, TimeSpan.Zero));

        await AddTenantAsync(user.TenantId);

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

    private async Task AddTenantAsync(TenantId tenantId)
    {
        await using var connection = new SqlConnection(fixture.ConnectionString);

        const string sql = """
            INSERT INTO [platform].[Tenants]
            (
                [Id],
                [Name],
                [Slug],
                [Status],
                [CreatedAtUtc],
                [UpdatedAtUtc],
                [IsDeleted]
            )
            VALUES
            (
                @Id,
                @Name,
                @Slug,
                @Status,
                @CreatedAtUtc,
                NULL,
                0
            );
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                Id = tenantId.Value,
                Name = $"Tenant {tenantId.Value:N}",
                Slug = tenantId.Value.ToString("N"),
                Status = 1,
                CreatedAtUtc = new DateTimeOffset(2026, 04, 30, 12, 0, 0, TimeSpan.Zero)
            });
    }

    private SqlUserRepository CreateRepository()
    {
        var connectionFactory = CreateConnectionFactory();

        return new SqlUserRepository(connectionFactory);
    }

    private IdentityDbConnectionFactory CreateConnectionFactory()
    {
        var options = Options.Create(new IdentityPersistenceOptions
        {
            ConnectionString = fixture.ConnectionString
        });

        return new IdentityDbConnectionFactory(options);
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

    private sealed record UserRecord(
        Guid Id,
        Guid TenantId,
        string Name,
        string Email,
        string NormalizedEmail,
        string PasswordHash,
        int Status);
}
