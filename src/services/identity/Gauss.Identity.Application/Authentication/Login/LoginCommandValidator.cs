using FluentValidation;
using Gauss.Identity.Domain.Users.ValueObjects;

namespace Gauss.Identity.Application.Authentication.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode("Identity.Login.EmailRequired")
            .MaximumLength(Email.MaxLength)
            .WithErrorCode("Identity.Login.EmailTooLong")
            .Must(email => Email.TryCreate(email, out _))
            .WithErrorCode("Identity.Login.EmailInvalid");

        RuleFor(command => command.Password)
            .NotEmpty()
            .WithErrorCode("Identity.Login.PasswordRequired");
    }
}
