using Gauss.BuildingBlocks.Domain.Entities;
using Gauss.Identity.Domain.Tenants;
using Gauss.Identity.Domain.Users.Events;
using Gauss.Identity.Domain.Users.ValueObjects;

namespace Gauss.Identity.Domain.Users;

public sealed class User : AggregateRoot<UserId>
{
    private User(
        UserId id,
        TenantId tenantId,
        string name,
        Email email,
        PasswordHash passwordHash,
        DateTimeOffset registeredAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        Status = UserStatus.PendingEmailConfirmation;
        RegisteredAtUtc = registeredAtUtc;

        RaiseDomainEvent(new UserRegisteredDomainEvent(
            Id,
            TenantId,
            Email.Value,
            RegisteredAtUtc));
    }

    private User(UserSnapshot snapshot)
    : base(snapshot.Id)
    {
        TenantId = snapshot.TenantId;
        Name = snapshot.Name;
        Email = snapshot.Email;
        PasswordHash = snapshot.PasswordHash;
        Status = snapshot.Status;
        RegisteredAtUtc = snapshot.RegisteredAtUtc;
        EmailConfirmedAtUtc = snapshot.EmailConfirmedAtUtc;
        LastLoginAtUtc = snapshot.LastLoginAtUtc;
        LockedUntilUtc = snapshot.LockedUntilUtc;
    }

    public TenantId TenantId { get; private init; }

    public string Name { get; private set; }

    public Email Email { get; private init; }

    public PasswordHash PasswordHash { get; private set; }

    public UserStatus Status { get; private set; }

    public DateTimeOffset RegisteredAtUtc { get; private init; }

    public DateTimeOffset? EmailConfirmedAtUtc { get; private set; }

    public DateTimeOffset? LastLoginAtUtc { get; private set; }

    public DateTimeOffset? LockedUntilUtc { get; private set; }

    public bool IsEmailConfirmed => EmailConfirmedAtUtc.HasValue;

    public bool IsActive => Status == UserStatus.Active;

    public static User Register(
        TenantId tenantId,
        string name,
        Email email,
        PasswordHash passwordHash,
        DateTimeOffset registeredAtUtc)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new User(
            UserId.New(),
            tenantId,
            name.Trim(),
            email,
            passwordHash,
            registeredAtUtc);
    }

    public static User Rehydrate(UserSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshot.Name);
        ArgumentNullException.ThrowIfNull(snapshot.Email);
        ArgumentNullException.ThrowIfNull(snapshot.PasswordHash);

        return new User(snapshot);
    }

    public bool CanAuthenticate(DateTimeOffset utcNow)
    {
        if (Status == UserStatus.Locked && LockedUntilUtc > utcNow)
        {
            return false;
        }

        return Status == UserStatus.Active;
    }

    public void ConfirmEmail(DateTimeOffset confirmedAtUtc)
    {
        if (IsEmailConfirmed)
        {
            return;
        }

        EmailConfirmedAtUtc = confirmedAtUtc;
        Status = UserStatus.Active;
    }

    public void RegisterSuccessfulLogin(DateTimeOffset loggedInAtUtc)
    {
        LastLoginAtUtc = loggedInAtUtc;
    }

    public void LockUntil(DateTimeOffset lockedUntilUtc)
    {
        if (lockedUntilUtc <= DateTimeOffset.UtcNow)
        {
            throw new ArgumentException(
                "Lock expiration must be in the future.",
                nameof(lockedUntilUtc));
        }

        LockedUntilUtc = lockedUntilUtc;
        Status = UserStatus.Locked;
    }

    public void Suspend()
    {
        Status = UserStatus.Suspended;
    }

    public void Deactivate()
    {
        Status = UserStatus.Deactivated;
    }
}
