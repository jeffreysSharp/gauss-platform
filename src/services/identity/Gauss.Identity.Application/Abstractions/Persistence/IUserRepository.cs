using Gauss.Identity.Domain.Users;
using Gauss.Identity.Domain.Users.ValueObjects;

namespace Gauss.Identity.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(
        Email email,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        User user,
        CancellationToken cancellationToken = default);
}
