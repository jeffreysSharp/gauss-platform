namespace Gauss.Identity.Application.Abstractions.Authentication;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }

    Guid? UserId { get; }

    Guid? TenantId { get; }

    string? Name { get; }

    string? Email { get; }
}
