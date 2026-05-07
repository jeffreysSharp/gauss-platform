namespace Gauss.Identity.Domain.Roles.ValueObjects;

public sealed record RoleName
{
    public const int MaxLength = 100;

    private RoleName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static RoleName Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength)
        {
            throw new ArgumentException(
                $"Role name cannot exceed {MaxLength} characters.",
                nameof(value));
        }

        return new RoleName(normalizedValue);
    }

    public override string ToString()
    {
        return Value;
    }
}
