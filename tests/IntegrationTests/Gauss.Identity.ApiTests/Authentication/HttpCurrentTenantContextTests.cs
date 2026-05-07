using AwesomeAssertions;
using Gauss.Identity.Api.Authentication;
using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Domain.Tenants;

namespace Gauss.Identity.ApiTests.Authentication;

public sealed class HttpCurrentTenantContextTests
{
    [Fact(DisplayName = "Should return tenant context when current user has tenant id")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Authentication")]
    public void Should_Return_Tenant_Context_When_CurrentUser_Has_TenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var currentUserContext = new FakeCurrentUserContext
        {
            TenantId = tenantId
        };

        var tenantContext = new HttpCurrentTenantContext(currentUserContext);

        // Act & Assert
        tenantContext.HasTenant.Should().BeTrue();
        tenantContext.CurrentTenantId.Should().Be(TenantId.From(tenantId));
    }

    [Fact(DisplayName = "Should return no tenant context when current user has no tenant id")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Authentication")]
    public void Should_Return_No_Tenant_Context_When_CurrentUser_Has_No_TenantId()
    {
        // Arrange
        var currentUserContext = new FakeCurrentUserContext
        {
            TenantId = null
        };

        var tenantContext = new HttpCurrentTenantContext(currentUserContext);

        // Act & Assert
        tenantContext.HasTenant.Should().BeFalse();
        tenantContext.CurrentTenantId.Should().BeNull();
    }

    private sealed class FakeCurrentUserContext : ICurrentUserContext
    {
        public bool IsAuthenticated { get; init; } = true;

        public Guid? UserId { get; init; } = Guid.NewGuid();

        public Guid? TenantId { get; init; }

        public string? Name { get; init; } = "Jeferson Almeida";

        public string? Email { get; init; } = "jeferson@gauss.com";
    }
}
