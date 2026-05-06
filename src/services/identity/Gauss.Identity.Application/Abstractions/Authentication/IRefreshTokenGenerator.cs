using Gauss.Identity.Application.Authentication.RefreshTokens;

namespace Gauss.Identity.Application.Abstractions.Authentication;

public interface IRefreshTokenGenerator
{
    RefreshToken Generate(DateTimeOffset issuedAtUtc);
}
