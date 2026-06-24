using FluentValidation;
using MovieAPI.Application.Models;

namespace MovieAPI.Application.validators;

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordDto>
{
  public ForgotPasswordValidator()
  {
    RuleFor(f => f.Email).NotEmpty().EmailAddress();
  }
}
