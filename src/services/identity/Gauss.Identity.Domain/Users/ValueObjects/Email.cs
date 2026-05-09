using System.Diagnostics.CodeAnalysis;
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
        if (!TryCreate(value, out var email))
        {
            throw new ArgumentException("Invalid email format.", nameof(value));
        }

        return email;
    }

    public static bool TryCreate(
        string? value,
        [NotNullWhen(true)] out Email? email)
    {
        email = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalizedEmail = Normalize(value);

        if (!EmailRegex().IsMatch(normalizedEmail))
        {
            return false;
        }

        email = new Email(normalizedEmail);

        return true;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToLowerInvariant();
    }

    [GeneratedRegex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.CultureInvariant,
        matchTimeoutMilliseconds: 500)]
    private static partial Regex EmailRegex();
}
