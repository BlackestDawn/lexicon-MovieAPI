using FluentValidation;
using MovieAPI.Application.Models;

namespace MovieAPI.Application.validators;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
{
  public ChangePasswordValidator()
  {
    RuleFor(c => c.CurrentPassword).NotEmpty();
    RuleFor(c => c.NewPassword).NotEmpty().NotEqual(c => c.CurrentPassword);
  }
}
