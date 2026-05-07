using AwesomeAssertions;
using Gauss.Identity.Domain.Tenants;

namespace Gauss.Identity.UnitTests.Domain.Users;

public sealed class TenantIdTests
{
    [Fact(DisplayName = "Should create new tenant id")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Identifiers")]
    public void Should_Create_New_TenantId()
    {
        // Act
        var tenantId = TenantId.New();

        // Assert
        tenantId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "Should create tenant id from valid guid")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Identifiers")]
    public void Should_Create_TenantId_From_Valid_Guid()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var tenantId = TenantId.From(value);

        // Assert
        tenantId.Value.Should().Be(value);
    }

    [Fact(DisplayName = "Should throw argument exception when tenant id is empty")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Identifiers")]
    public void Should_Throw_ArgumentException_When_TenantId_Is_Empty()
    {
        // Act
        var action = () => TenantId.From(Guid.Empty);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Should return guid value as string")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Identifiers")]
    public void Should_Return_Guid_Value_As_String()
    {
        // Arrange
        var value = Guid.NewGuid();
        var tenantId = TenantId.From(value);

        // Act
        var result = tenantId.ToString();

        // Assert
        result.Should().Be(value.ToString());
    }
}
