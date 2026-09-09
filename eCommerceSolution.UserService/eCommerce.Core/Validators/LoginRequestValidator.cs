using eCommerce.Core.DTO;
using FluentValidation;

namespace eCommerce.Core.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        //Email
        RuleFor(model => model.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address format");

        //Password
        RuleFor(model => model.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
