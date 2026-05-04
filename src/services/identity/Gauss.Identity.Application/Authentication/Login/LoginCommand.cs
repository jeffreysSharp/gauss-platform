using Gauss.BuildingBlocks.Application.Abstractions.Messaging;

namespace Gauss.Identity.Application.Authentication.Login;

public sealed record LoginCommand(
    string Email,
    string Password) :
    ICommand<LoginResponse>;
