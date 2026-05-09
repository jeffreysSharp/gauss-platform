namespace Gauss.BuildingBlocks.Domain.Tenants;

public readonly record struct TenantId(Guid Value)
{
    public static TenantId New()
    {
        return new TenantId(Guid.NewGuid());
    }

    public static TenantId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("TenantId cannot be empty.", nameof(value));
        }

        return new TenantId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
