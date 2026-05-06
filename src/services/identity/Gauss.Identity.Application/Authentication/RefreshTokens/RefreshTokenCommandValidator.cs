using FluentValidation;

namespace Gauss.Identity.Application.Authentication.RefreshTokens;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(command => command.RefreshToken)
            .NotEmpty()
            .WithErrorCode("Identity.RefreshToken.Required");
    }
}
