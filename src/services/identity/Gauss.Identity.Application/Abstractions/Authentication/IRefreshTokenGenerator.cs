namespace Gauss.Identity.Application.Abstractions.Authentication;

public interface IRefreshTokenGenerator
{
    RefreshToken Generate(DateTimeOffset issuedAtUtc);
}
