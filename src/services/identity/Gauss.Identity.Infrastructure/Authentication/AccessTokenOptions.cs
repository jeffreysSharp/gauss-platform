using System;
using System.Collections.Generic;
using System.Text;

namespace Gauss.Identity.Infrastructure.Authentication;

public sealed class AccessTokenOptions
{
    public const string SectionName = "Identity:AccessToken";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string SecretKey { get; init; } = string.Empty;

    public int ExpirationMinutes { get; init; } = 15;
}
