using FluentValidation;

namespace Gauss.Identity.Application.Users.RegisterUser;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .WithErrorCode("Identity.User.NameRequired")
            .MaximumLength(150)
            .WithErrorCode("Identity.User.NameTooLong");

        RuleFor(command => command.Email)
            .NotEmpty()
            .WithErrorCode("Identity.User.EmailRequired")
            .EmailAddress()
            .WithErrorCode("Identity.User.EmailInvalid")
            .MaximumLength(254)
            .WithErrorCode("Identity.User.EmailTooLong");

        RuleFor(command => command.Password)
            .NotEmpty()
            .WithErrorCode("Identity.User.PasswordRequired")
            .MinimumLength(8)
            .WithErrorCode("Identity.User.PasswordTooShort")
            .Matches("[A-Z]")
            .WithErrorCode("Identity.User.PasswordRequiresUppercase")
            .Matches("[a-z]")
            .WithErrorCode("Identity.User.PasswordRequiresLowercase")
            .Matches("[0-9]")
            .WithErrorCode("Identity.User.PasswordRequiresDigit")
            .Matches("[^a-zA-Z0-9]")
            .WithErrorCode("Identity.User.PasswordRequiresNonAlphanumeric");
    }
}
