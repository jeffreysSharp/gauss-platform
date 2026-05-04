using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AwesomeAssertions;
using Gauss.Identity.Application.Abstractions.Time;
using Gauss.Identity.Domain.Users;
using Gauss.Identity.Domain.Users.Tenancy;
using Gauss.Identity.Domain.Users.ValueObjects;
using Gauss.Identity.Infrastructure.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Gauss.Identity.InfrastructureTests.Authentication;

public sealed class JwtAccessTokenProviderTests
{
    [Fact(DisplayName = "Should generate access token when user is valid")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Generate_AccessToken_When_User_Is_Valid()
    {
        // Arrange
        var dateTimeProvider = new FakeDateTimeProvider();
        var options = CreateOptions();

        var provider = new JwtAccessTokenProvider(
            options,
            dateTimeProvider);

        var user = CreateActiveUser();

        // Act
        var accessToken = provider.Generate(user);

        // Assert
        accessToken.Value.Should().NotBeNullOrWhiteSpace();
        accessToken.TokenType.Should().Be("Bearer");
        accessToken.ExpiresAtUtc.Should().Be(
            dateTimeProvider.UtcNow.AddMinutes(options.Value.ExpirationMinutes));
    }

    [Fact(DisplayName = "Should generate token with expected claims")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Generate_Token_With_Expected_Claims()
    {
        // Arrange
        var dateTimeProvider = new FakeDateTimeProvider();
        var options = CreateOptions();

        var provider = new JwtAccessTokenProvider(
            options,
            dateTimeProvider);

        var user = CreateActiveUser();

        // Act
        var accessToken = provider.Generate(user);

        var jwt = new JwtSecurityTokenHandler()
            .ReadJwtToken(accessToken.Value);

        // Assert
        jwt.Claims.Should().Contain(claim =>
            claim.Type == JwtRegisteredClaimNames.Sub &&
            claim.Value == user.Id.Value.ToString());

        jwt.Claims.Should().Contain(claim =>
            claim.Type == JwtRegisteredClaimNames.Email &&
            claim.Value == user.Email.Value);

        jwt.Claims.Should().Contain(claim =>
            claim.Type == JwtRegisteredClaimNames.Name &&
            claim.Value == user.Name);

        jwt.Claims.Should().Contain(claim =>
            claim.Type == "tenant_id" &&
            claim.Value == user.TenantId.Value.ToString());

        jwt.Claims.Should().Contain(claim =>
            claim.Type == JwtRegisteredClaimNames.Jti &&
            !string.IsNullOrWhiteSpace(claim.Value));
    }

    [Fact(DisplayName = "Should generate token with expected issuer and audience")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Generate_Token_With_Expected_Issuer_And_Audience()
    {
        // Arrange
        var provider = new JwtAccessTokenProvider(
            CreateOptions(),
            new FakeDateTimeProvider());

        var user = CreateActiveUser();

        // Act
        var accessToken = provider.Generate(user);

        var jwt = new JwtSecurityTokenHandler()
            .ReadJwtToken(accessToken.Value);

        // Assert
        jwt.Issuer.Should().Be("GAUSS.Identity");
        jwt.Audiences.Should().ContainSingle()
            .Which.Should().Be("GAUSS.Platform");
    }

    [Fact(DisplayName = "Should validate generated token signature")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Validate_Generated_Token_Signature()
    {
        // Arrange
        var options = CreateOptions();

        var provider = new JwtAccessTokenProvider(
            options,
            new FakeDateTimeProvider());

        var user = CreateActiveUser();

        var tokenHandler = new JwtSecurityTokenHandler();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = options.Value.Issuer,
            ValidateAudience = true,
            ValidAudience = options.Value.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(options.Value.SecretKey)),
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero
        };

        // Act
        var accessToken = provider.Generate(user);

        var principal = tokenHandler.ValidateToken(
            accessToken.Value,
            validationParameters,
            out var validatedToken);

        // Assert
        principal.Should().NotBeNull();
        validatedToken.Should().BeOfType<JwtSecurityToken>();
    }   

    [Fact(DisplayName = "Should throw argument null exception when user is null")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public void Should_Throw_ArgumentNullException_When_User_Is_Null()
    {
        // Arrange
        var provider = new JwtAccessTokenProvider(
            CreateOptions(),
            new FakeDateTimeProvider());

        // Act
        var action = () => provider.Generate(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    private static IOptions<AccessTokenOptions> CreateOptions()
    {
        return Options.Create(new AccessTokenOptions
        {
            Issuer = "GAUSS.Identity",
            Audience = "GAUSS.Platform",
            SecretKey = "development-only-secret-key-with-at-least-32-characters",
            ExpirationMinutes = 15
        });
    }

    private static User CreateActiveUser()
    {
        var user = User.Register(
            TenantId.New(),
            "Jeferson Almeida",
            Email.Create("jeferson@gauss.com"),
            PasswordHash.Create("hashed-password"),
            new DateTimeOffset(2026, 04, 30, 12, 0, 0, TimeSpan.Zero));

        user.ConfirmEmail(new DateTimeOffset(2026, 04, 30, 12, 5, 0, TimeSpan.Zero));

        return user;
    }

    private sealed class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow { get; } =
            new(2026, 04, 30, 12, 30, 0, TimeSpan.Zero);
    }
}
