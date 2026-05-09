using AwesomeAssertions;
using Gauss.Identity.Api.Authorization;
using Gauss.Identity.Application.Abstractions.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Gauss.Identity.ApiTests.Authorization;

public sealed class PermissionEndpointFilterTests
{
    [Fact(DisplayName = "Should continue endpoint execution when user has permission")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Authorization")]
    public async Task Should_Continue_Endpoint_Execution_When_User_Has_Permission()
    {
        // Arrange
        var authorizationService = new FakePermissionAuthorizationService
        {
            HasPermission = true
        };

        var httpContext = CreateHttpContext(authorizationService);

        var filter = new PermissionEndpointFilter("Identity.Users.Read");

        var wasNextCalled = false;

        EndpointFilterDelegate next = _ =>
        {
            wasNextCalled = true;

            return ValueTask.FromResult<object?>(Results.Ok());
        };

        var invocationContext = new DefaultEndpointFilterInvocationContext(
            httpContext);

        // Act
        var result = await filter.InvokeAsync(
            invocationContext,
            next);

        // Assert
        wasNextCalled.Should().BeTrue();

        result.Should().BeAssignableTo<IResult>();

        authorizationService.LastPermissionCode.Should().Be("Identity.Users.Read");
    }

    [Fact(DisplayName = "Should return forbidden when user does not have permission")]
    [Trait("Layer", "Api")]
    [Trait("Category", "Authorization")]
    public async Task Should_Return_Forbidden_When_User_Does_Not_Have_Permission()
    {
        // Arrange
        var authorizationService = new FakePermissionAuthorizationService
        {
            HasPermission = false
        };

        var httpContext = CreateHttpContext(authorizationService);

        var filter = new PermissionEndpointFilter("Identity.Users.Manage");

        var wasNextCalled = false;

        EndpointFilterDelegate next = _ =>
        {
            wasNextCalled = true;

            return ValueTask.FromResult<object?>(Results.Ok());
        };

        var invocationContext = new DefaultEndpointFilterInvocationContext(
            httpContext);

        // Act
        var result = await filter.InvokeAsync(
            invocationContext,
            next);

        // Assert
        wasNextCalled.Should().BeFalse();

        result.Should().BeAssignableTo<IResult>();

        authorizationService.LastPermissionCode.Should().Be("Identity.Users.Manage");
    }

    private static DefaultHttpContext CreateHttpContext(
        IPermissionAuthorizationService permissionAuthorizationService)
    {
        var services = new ServiceCollection();

        services.AddScoped(_ => permissionAuthorizationService);

        var serviceProvider = services.BuildServiceProvider();

        return new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };
    }

    private sealed class FakePermissionAuthorizationService : IPermissionAuthorizationService
    {
        public bool HasPermission { get; init; }

        public string? LastPermissionCode { get; private set; }

        public Task<bool> HasPermissionAsync(
            string permissionCode,
            CancellationToken cancellationToken = default)
        {
            LastPermissionCode = permissionCode;

            return Task.FromResult(HasPermission);
        }
    }
}
