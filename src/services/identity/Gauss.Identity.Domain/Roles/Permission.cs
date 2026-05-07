using Gauss.BuildingBlocks.Domain.Entities;
using Gauss.Identity.Domain.Roles.ValueObjects;

namespace Gauss.Identity.Domain.Roles;

public sealed class Permission : AggregateRoot<PermissionId>
{
    private Permission(
        PermissionId id,
        PermissionCode code,
        string description,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        Code = code;
        Description = description;
        CreatedAtUtc = createdAtUtc;
        IsEnabled = true;
    }

    public PermissionCode Code { get; private init; }

    public string Description { get; private set; }

    public bool IsEnabled { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private init; }

    public static Permission Create(
        PermissionCode code,
        string description,
        DateTimeOffset createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new Permission(
            PermissionId.New(),
            code,
            description.Trim(),
            createdAtUtc);
    }

    public void UpdateDescription(string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Description = description.Trim();
    }

    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }
}
