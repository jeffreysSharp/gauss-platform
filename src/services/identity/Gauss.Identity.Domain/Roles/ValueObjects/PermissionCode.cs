namespace Gauss.Identity.Domain.Roles.ValueObjects;

public sealed record PermissionCode
{
    public const int MaxLength = 150;

    private PermissionCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PermissionCode Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength)
        {
            throw new ArgumentException(
                $"Permission code cannot exceed {MaxLength} characters.",
                nameof(value));
        }

        return new PermissionCode(normalizedValue);
    }

    public override string ToString()
    {
        return Value;
    }
}
