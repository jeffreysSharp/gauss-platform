using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Application.Abstractions.Time;
using Gauss.Identity.Domain.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Gauss.Identity.Infrastructure.Authentication;

public sealed class JwtAccessTokenProvider(
    IOptions<AccessTokenOptions> options,
    IDateTimeProvider dateTimeProvider)
    : IAccessTokenProvider
{
    private readonly AccessTokenOptions _options = options.Value;

    public AccessToken Generate(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        ValidateOptions();

        var issuedAtUtc = dateTimeProvider.UtcNow;
        var expiresAtUtc = issuedAtUtc.AddMinutes(_options.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email.Value),
            new(JwtRegisteredClaimNames.Name, user.Name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new("tenant_id", user.TenantId.Value.ToString())
        };

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.SecretKey));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: issuedAtUtc.UtcDateTime,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: credentials);

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(jwt);

        return new AccessToken(
            tokenValue,
            "Bearer",
            expiresAtUtc);
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.Issuer))
        {
            throw new InvalidOperationException("Access token issuer was not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.Audience))
        {
            throw new InvalidOperationException("Access token audience was not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            throw new InvalidOperationException("Access token secret key was not configured.");
        }

        if (_options.SecretKey.Length < 32)
        {
            throw new InvalidOperationException("Access token secret key must have at least 32 characters.");
        }

        if (_options.ExpirationMinutes <= 0)
        {
            throw new InvalidOperationException("Access token expiration must be greater than zero.");
        }
    }
}
