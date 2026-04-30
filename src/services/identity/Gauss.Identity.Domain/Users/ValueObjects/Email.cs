using System.Text.RegularExpressions;
using Gauss.BuildingBlocks.Domain.ValueObjects;

namespace Gauss.Identity.Domain.Users.ValueObjects;

public sealed partial class Email : ValueObject
{
    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Email Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalizedEmail = value.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(normalizedEmail))
        {
            throw new ArgumentException("Invalid email format.", nameof(value));
        }

        return new Email(normalizedEmail);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }

    [GeneratedRegex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.CultureInvariant,
        matchTimeoutMilliseconds: 500)]
    private static partial Regex EmailRegex();
}
