using FluentValidation;

namespace Gauss.Identity.Application.Authentication.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .WithErrorCode("Identity.Login.EmailRequired")
            .EmailAddress()
            .WithErrorCode("Identity.Login.EmailInvalid")
            .MaximumLength(254)
            .WithErrorCode("Identity.Login.EmailTooLong");

        RuleFor(command => command.Password)
            .NotEmpty()
            .WithErrorCode("Identity.Login.PasswordRequired");
    }
}
