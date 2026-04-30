using Gauss.BuildingBlocks.Domain.Events;

namespace Gauss.Identity.Domain.Users.Events;

public sealed record UserRegisteredDomainEvent(
    UserId UserId,
    TenantId TenantId,
    string Email,
    DateTimeOffset OccurredOnUtc)
    : IDomainEvent;
