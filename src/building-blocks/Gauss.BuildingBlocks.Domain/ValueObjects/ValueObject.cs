namespace Gauss.BuildingBlocks.Domain.ValueObjects;

public abstract class ValueObject
{
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;

        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(
                HashCode.Combine(GetType()),
                HashCode.Combine);
    }

    protected abstract IEnumerable<object?> GetEqualityComponents();
}
