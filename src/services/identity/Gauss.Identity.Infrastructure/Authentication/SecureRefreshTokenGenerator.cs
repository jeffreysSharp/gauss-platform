using System.Security.Cryptography;
using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Application.Authentication.RefreshTokens;
using Microsoft.Extensions.Options;

namespace Gauss.Identity.Infrastructure.Authentication;

public sealed class SecureRefreshTokenGenerator(
    IOptions<RefreshTokenOptions> options)
    : IRefreshTokenGenerator
{
    private const int TokenByteLength = 64;

    private readonly RefreshTokenOptions _options = options.Value;

    public RefreshToken Generate(DateTimeOffset issuedAtUtc)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(TokenByteLength);

        var tokenValue = Base64UrlEncode(randomBytes);

        var expiresAtUtc = issuedAtUtc.AddMinutes(_options.ExpirationMinutes);

        return new RefreshToken(
            tokenValue,
            expiresAtUtc);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
