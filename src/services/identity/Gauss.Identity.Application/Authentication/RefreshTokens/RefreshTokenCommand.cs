using Gauss.BuildingBlocks.Application.Abstractions.Messaging;

namespace Gauss.Identity.Application.Authentication.RefreshTokens;

public sealed record RefreshTokenCommand(
    string RefreshToken)
    : ICommand<RefreshTokenResponse>;
