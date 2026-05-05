using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Gauss.Identity.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Gauss.Identity.Api.Authentication;

public sealed class JwtBearerOptionsSetup(
    IOptions<AccessTokenOptions> accessTokenOptions)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(
        string? name,
        JwtBearerOptions options)
    {
        if (!string.Equals(
                name,
                JwtBearerDefaults.AuthenticationScheme,
                StringComparison.Ordinal))
        {
            return;
        }

        var tokenOptions = accessTokenOptions.Value;

        options.RequireHttpsMetadata = true;
        options.SaveToken = false;
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = tokenOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = tokenOptions.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(tokenOptions.SecretKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),

            NameClaimType = JwtRegisteredClaimNames.Name,
            RoleClaimType = ClaimTypes.Role
        };
    }

    public void Configure(
        JwtBearerOptions options)
    {
        Configure(
            JwtBearerDefaults.AuthenticationScheme,
            options);
    }
}
