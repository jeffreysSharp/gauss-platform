using AwesomeAssertions;
using Gauss.Identity.Domain.Tenants;
using Gauss.Identity.Domain.Users;
using Gauss.Identity.Domain.Users.Events;
using Gauss.Identity.Domain.Users.ValueObjects;

namespace Gauss.Identity.UnitTests.Domain.Users;

public sealed class UserTests
{
    [Fact(DisplayName = "Should create user when input is valid")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Aggregates")]
    public void Should_Create_User_When_Input_Is_Valid()
    {
        // Arrange
        var tenantId = TenantId.New();
        const string name = "Jeferson Almeida";
        var email = Email.Create("jeferson@gauss.com");
        var passwordHash = PasswordHash.Create("hashed-password");
        var registeredAtUtc = new DateTimeOffset(2026, 04, 30, 12, 0, 0, TimeSpan.Zero);

        // Act
        var user = User.Register(
            tenantId,
            name,
            email,
            passwordHash,
            registeredAtUtc);

        // Assert
        user.Id.Value.Should().NotBe(Guid.Empty);
        user.TenantId.Should().Be(tenantId);
        user.Name.Should().Be(name);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(passwordHash);
        user.Status.Should().Be(UserStatus.PendingEmailConfirmation);
        user.RegisteredAtUtc.Should().Be(registeredAtUtc);
        user.EmailConfirmedAtUtc.Should().BeNull();
        user.LastLoginAtUtc.Should().BeNull();
        user.LockedUntilUtc.Should().BeNull();
        user.IsEmailConfirmed.Should().BeFalse();
        user.IsActive.Should().BeFalse();
    }

    [Fact(DisplayName = "Should trim user name when name has extra spaces")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Aggregates")]
    public void Should_Trim_User_Name_When_Name_Has_Extra_Spaces()
    {
        // Arrange
        var tenantId = TenantId.New();
        var email = Email.Create("jeferson@gauss.com");
        var passwordHash = PasswordHash.Create("hashed-password");
        var registeredAtUtc = new DateTimeOffset(2026, 04, 30, 12, 0, 0, TimeSpan.Zero);

        // Act
        var user = User.Register(
            tenantId,
            "  Jeferson Almeida  ",
            email,
            passwordHash,
            registeredAtUtc);

        // Assert
        user.Name.Should().Be("Jeferson Almeida");
    }

    [Theory(DisplayName = "Should throw argument exception when user name is invalid")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Aggregates")]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_Throw_ArgumentException_When_User_Name_Is_Invalid(string name)
    {
        // Arrange
        var tenantId = TenantId.New();
        var email = Email.Create("jeferson@gauss.com");
        var passwordHash = PasswordHash.Create("hashed-password");
        var registeredAtUtc = new DateTimeOffset(2026, 04, 30, 12, 0, 0, TimeSpan.Zero);

        // Act
        var action = () => User.Register(
            tenantId,
            name,
            email,
            passwordHash,
            registeredAtUtc);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Should raise user registered domain event when user is registered")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "DomainEvents")]
    public void Should_Raise_UserRegisteredDomainEvent_When_User_Is_Registered()
    {
        // Arrange
        var tenantId = TenantId.New();
        var email = Email.Create("jeferson@gauss.com");
        var passwordHash = PasswordHash.Create("hashed-password");
        var registeredAtUtc = new DateTimeOffset(2026, 04, 30, 12, 0, 0, TimeSpan.Zero);

        // Act
        var user = User.Register(
            tenantId,
            "Jeferson Almeida",
            email,
            passwordHash,
            registeredAtUtc);

        // Assert
        user.DomainEvents.Should().ContainSingle();

        var domainEvent = user.DomainEvents.Single();
        var userRegisteredEvent = domainEvent.Should()
            .BeOfType<UserRegisteredDomainEvent>()
            .Subject;

        userRegisteredEvent.UserId.Should().Be(user.Id);
        userRegisteredEvent.TenantId.Should().Be(tenantId);
        userRegisteredEvent.Email.Should().Be(email.Value);
        userRegisteredEvent.OccurredOnUtc.Should().Be(registeredAtUtc);
    }

    [Fact(DisplayName = "Should activate user when email is confirmed")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Aggregates")]
    public void Should_Activate_User_When_Email_Is_Confirmed()
    {
        // Arrange
        var user = CreateUser();
        var confirmedAtUtc = new DateTimeOffset(2026, 04, 30, 13, 0, 0, TimeSpan.Zero);

        // Act
        user.ConfirmEmail(confirmedAtUtc);

        // Assert
        user.IsEmailConfirmed.Should().BeTrue();
        user.IsActive.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
        user.EmailConfirmedAtUtc.Should().Be(confirmedAtUtc);
    }

    [Fact(DisplayName = "Should not change email confirmed date when email is already confirmed")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Aggregates")]
    public void Should_Not_Change_EmailConfirmedAtUtc_When_Email_Is_Already_Confirmed()
    {
        // Arrange
        var user = CreateUser();
        var firstConfirmedAtUtc = new DateTimeOffset(2026, 04, 30, 13, 0, 0, TimeSpan.Zero);
        var secondConfirmedAtUtc = new DateTimeOffset(2026, 04, 30, 14, 0, 0, TimeSpan.Zero);

        user.ConfirmEmail(firstConfirmedAtUtc);

        // Act
        user.ConfirmEmail(secondConfirmedAtUtc);

        // Assert
        user.EmailConfirmedAtUtc.Should().Be(firstConfirmedAtUtc);
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact(DisplayName = "Should update last login date when login is successful")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Aggregates")]
    public void Should_Update_LastLoginAtUtc_When_Login_Is_Successful()
    {
        // Arrange
        var user = CreateUser();
        var loggedInAtUtc = new DateTimeOffset(2026, 04, 30, 14, 0, 0, TimeSpan.Zero);

        // Act
        user.RegisterSuccessfulLogin(loggedInAtUtc);

        // Assert
        user.LastLoginAtUtc.Should().Be(loggedInAtUtc);
    }

    [Fact(DisplayName = "Should lock user until informed date")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Aggregates")]
    public void Should_Lock_User_Until_Informed_Date()
    {
        // Arrange
        var user = CreateUser();
        var utcNow = new DateTimeOffset(2026, 04, 30, 12, 0, 0, TimeSpan.Zero);
        var lockedUntilUtc = utcNow.AddMinutes(15);

        // Act
        user.LockUntil(lockedUntilUtc, utcNow);

        // Assert
        user.Status.Should().Be(UserStatus.Locked);
        user.LockedUntilUtc.Should().Be(lockedUntilUtc);
    }

    [Fact(DisplayName = "Should throw argument exception when lock expiration is not in the future")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Aggregates")]
    public void Should_Throw_ArgumentException_When_Lock_Expiration_Is_Not_In_The_Future()
    {
        // Arrange
        var user = CreateUser();
        var utcNow = new DateTimeOffset(2026, 04, 30, 12, 0, 0, TimeSpan.Zero);
        var lockedUntilUtc = utcNow.AddMinutes(-1);

        // Act
        var action = () => user.LockUntil(lockedUntilUtc, utcNow);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Should throw argument exception when lock expiration equals utcNow")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Aggregates")]
    public void Should_Throw_ArgumentException_When_Lock_Expiration_Equals_UtcNow()
    {
        // Arrange
        var user = CreateUser();
        var utcNow = new DateTimeOffset(2026, 04, 30, 12, 0, 0, TimeSpan.Zero);

        // Act
        var action = () => user.LockUntil(utcNow, utcNow);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Should set status to suspended when user is suspended")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Aggregates")]
    public void Should_Set_Status_To_Suspended_When_User_Is_Suspended()
    {
        // Arrange
        var user = CreateUser();

        // Act
        user.Suspend();

        // Assert
        user.Status.Should().Be(UserStatus.Suspended);
    }

    [Fact(DisplayName = "Should set status to deactivated when user is deactivated")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Aggregates")]
    public void Should_Set_Status_To_Deactivated_When_User_Is_Deactivated()
    {
        // Arrange
        var user = CreateUser();

        // Act
        user.Deactivate();

        // Assert
        user.Status.Should().Be(UserStatus.Deactivated);
    }

    private static User CreateUser()
    {
        return User.Register(
            TenantId.New(),
            "Jeferson Almeida",
            Email.Create("jeferson@gauss.com"),
            PasswordHash.Create("hashed-password"),
            new DateTimeOffset(2026, 04, 30, 12, 0, 0, TimeSpan.Zero));
    }
}
