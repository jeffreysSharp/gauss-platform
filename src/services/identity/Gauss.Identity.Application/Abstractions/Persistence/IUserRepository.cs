using Gauss.Identity.Domain.Users;
using Gauss.Identity.Domain.Users.ValueObjects;

namespace Gauss.Identity.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(
        Email email,
        CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(
        Email email,
        CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        User user,
        CancellationToken cancellationToken = default);

    Task RecordLoginAsync(
        UserId userId,
        DateTimeOffset loggedInAtUtc,
        CancellationToken cancellationToken = default);
}
