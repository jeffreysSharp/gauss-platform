using Gauss.BuildingBlocks.Application.Abstractions.Messaging;

namespace Gauss.Identity.Application.Users.RegisterUser;

public sealed record RegisterUserCommand(
    string Name,
    string Email,
    string Password) : ICommand<RegisterUserResponse>;
