using Gauss.BuildingBlocks.Domain.ValueObjects;

namespace Gauss.Identity.Domain.Users.ValueObjects;

public sealed class PasswordHash : ValueObject
{
    private PasswordHash(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PasswordHash Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return new PasswordHash(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return "[PROTECTED]";
    }
}
