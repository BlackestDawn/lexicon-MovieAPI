using FluentValidation;
using MovieAPI.Application.Models;

namespace MovieAPI.Application.validators;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
{
  public ResetPasswordValidator()
  {
    RuleFor(r => r.Email).NotEmpty().EmailAddress();
    RuleFor(r => r.Token).NotEmpty();
    RuleFor(r => r.NewPassword).NotEmpty();
  }
}
