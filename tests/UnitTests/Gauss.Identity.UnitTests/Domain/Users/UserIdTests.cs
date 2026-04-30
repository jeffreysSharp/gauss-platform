using System;
using System.Collections.Generic;
using System.Text;
using AwesomeAssertions;
using Gauss.Identity.Domain.Users;

namespace Gauss.Identity.UnitTests.Domain.Users;

public sealed class UserIdTests
{
    [Fact(DisplayName = "Should create new user id")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Identifiers")]
    public void Should_Create_New_UserId()
    {
        // Act
        var userId = UserId.New();

        // Assert
        userId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "Should create user id from valid guid")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Identifiers")]
    public void Should_Create_UserId_From_Valid_Guid()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var userId = UserId.From(value);

        // Assert
        userId.Value.Should().Be(value);
    }

    [Fact(DisplayName = "Should throw argument exception when user id is empty")]
    [Trait("Layer", "Domain")]
    [Trait("Category", "Identifiers")]
    public void Should_Throw_ArgumentException_When_UserId_Is_Empty()
    {
        // Act
        var action = () => UserId.From(Guid.Empty);

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
        var userId = UserId.From(value);

        // Act
        var result = userId.ToString();

        // Assert
        result.Should().Be(value.ToString());
    }
}
