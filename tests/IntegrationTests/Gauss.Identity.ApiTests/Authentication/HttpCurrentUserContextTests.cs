using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AwesomeAssertions;
using Gauss.Identity.Api.Authentication;
using Gauss.Identity.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace Gauss.Identity.ApiTests.Authentication;

public sealed class HttpCurrentUserContextTests
{
    [Fact(DisplayName = "Should return authenticated user context when claims are valid")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Authentication")]
    public void Should_Return_Authenticated_User_Context_When_Claims_Are_Valid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var httpContextAccessor = CreateHttpContextAccessor(
            isAuthenticated: true,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(GaussClaimTypes.TenantId, tenantId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, "Jeferson Almeida"),
                new Claim(JwtRegisteredClaimNames.Email, "jeferson@gauss.com")
            ]);

        var currentUserContext = new HttpCurrentUserContext(httpContextAccessor);

        // Act & Assert
        currentUserContext.IsAuthenticated.Should().BeTrue();
        currentUserContext.UserId.Should().Be(userId);
        currentUserContext.TenantId.Should().Be(tenantId);
        currentUserContext.Name.Should().Be("Jeferson Almeida");
        currentUserContext.Email.Should().Be("jeferson@gauss.com");
    }

    [Fact(DisplayName = "Should return unauthenticated context when identity is not authenticated")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Authentication")]
    public void Should_Return_Unauthenticated_Context_When_Identity_Is_Not_Authenticated()
    {
        // Arrange
        var httpContextAccessor = CreateHttpContextAccessor(
            isAuthenticated: false,
            claims: []);

        var currentUserContext = new HttpCurrentUserContext(httpContextAccessor);

        // Act & Assert
        currentUserContext.IsAuthenticated.Should().BeFalse();
        currentUserContext.UserId.Should().BeNull();
        currentUserContext.TenantId.Should().BeNull();
        currentUserContext.Name.Should().BeNull();
        currentUserContext.Email.Should().BeNull();
    }

    [Fact(DisplayName = "Should return null user id when sub claim is missing")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Authentication")]
    public void Should_Return_Null_UserId_When_Sub_Claim_Is_Missing()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var httpContextAccessor = CreateHttpContextAccessor(
            isAuthenticated: true,
            claims:
            [
                new Claim(GaussClaimTypes.TenantId, tenantId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, "Jeferson Almeida"),
                new Claim(JwtRegisteredClaimNames.Email, "jeferson@gauss.com")
            ]);

        var currentUserContext = new HttpCurrentUserContext(httpContextAccessor);

        // Act & Assert
        currentUserContext.IsAuthenticated.Should().BeTrue();
        currentUserContext.UserId.Should().BeNull();
        currentUserContext.TenantId.Should().Be(tenantId);
    }

    [Fact(DisplayName = "Should return null user id when sub claim is invalid")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Authentication")]
    public void Should_Return_Null_UserId_When_Sub_Claim_Is_Invalid()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var httpContextAccessor = CreateHttpContextAccessor(
            isAuthenticated: true,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, "invalid-user-id"),
                new Claim(GaussClaimTypes.TenantId, tenantId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, "Jeferson Almeida"),
                new Claim(JwtRegisteredClaimNames.Email, "jeferson@gauss.com")
            ]);

        var currentUserContext = new HttpCurrentUserContext(httpContextAccessor);

        // Act & Assert
        currentUserContext.IsAuthenticated.Should().BeTrue();
        currentUserContext.UserId.Should().BeNull();
        currentUserContext.TenantId.Should().Be(tenantId);
    }

    [Fact(DisplayName = "Should return null tenant id when tenant claim is missing")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Authentication")]
    public void Should_Return_Null_TenantId_When_Tenant_Claim_Is_Missing()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var httpContextAccessor = CreateHttpContextAccessor(
            isAuthenticated: true,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, "Jeferson Almeida"),
                new Claim(JwtRegisteredClaimNames.Email, "jeferson@gauss.com")
            ]);

        var currentUserContext = new HttpCurrentUserContext(httpContextAccessor);

        // Act & Assert
        currentUserContext.IsAuthenticated.Should().BeTrue();
        currentUserContext.UserId.Should().Be(userId);
        currentUserContext.TenantId.Should().BeNull();
    }

    [Fact(DisplayName = "Should return null tenant id when tenant claim is invalid")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Authentication")]
    public void Should_Return_Null_TenantId_When_Tenant_Claim_Is_Invalid()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var httpContextAccessor = CreateHttpContextAccessor(
            isAuthenticated: true,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(GaussClaimTypes.TenantId, "invalid-tenant-id"),
                new Claim(JwtRegisteredClaimNames.Name, "Jeferson Almeida"),
                new Claim(JwtRegisteredClaimNames.Email, "jeferson@gauss.com")
            ]);

        var currentUserContext = new HttpCurrentUserContext(httpContextAccessor);

        // Act & Assert
        currentUserContext.IsAuthenticated.Should().BeTrue();
        currentUserContext.UserId.Should().Be(userId);
        currentUserContext.TenantId.Should().BeNull();
    }

    [Fact(DisplayName = "Should return null values when http context is missing")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Authentication")]
    public void Should_Return_Null_Values_When_HttpContext_Is_Missing()
    {
        // Arrange
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = null
        };

        var currentUserContext = new HttpCurrentUserContext(httpContextAccessor);

        // Act & Assert
        currentUserContext.IsAuthenticated.Should().BeFalse();
        currentUserContext.UserId.Should().BeNull();
        currentUserContext.TenantId.Should().BeNull();
        currentUserContext.Name.Should().BeNull();
        currentUserContext.Email.Should().BeNull();
    }

    private static HttpContextAccessor CreateHttpContextAccessor(
    bool isAuthenticated,
    IReadOnlyCollection<Claim> claims)
    {
        var identity = isAuthenticated
            ? new ClaimsIdentity(claims, authenticationType: "Test")
            : new ClaimsIdentity(claims);

        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        return new HttpContextAccessor
        {
            HttpContext = httpContext
        };
    }
}
