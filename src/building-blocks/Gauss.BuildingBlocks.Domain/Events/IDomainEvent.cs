namespace Gauss.BuildingBlocks.Domain.Events;

public interface IDomainEvent
{
    DateTimeOffset OccurredOnUtc { get; }
}
