namespace Gauss.Identity.Domain.Roles;

public readonly record struct PermissionId(Guid Value)
{
    public static PermissionId New()
    {
        return new PermissionId(Guid.NewGuid());
    }

    public static PermissionId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Permission id cannot be empty.",
                nameof(value));
        }

        return new PermissionId(value);
    }
}
