using Gauss.Identity.Domain.Users;

namespace Gauss.Identity.Application.Abstractions.Authentication;

public interface IAccessTokenProvider
{
    AccessToken Generate(User user);
}
