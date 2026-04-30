namespace Gauss.Identity.Domain.Users;

public readonly record struct UserId(Guid Value)
{
    public static UserId New()
    {
        return new UserId(Guid.NewGuid());
    }

    public static UserId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("UserId cannot be empty.", nameof(value));
        }

        return new UserId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
