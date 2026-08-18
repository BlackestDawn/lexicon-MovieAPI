using FluentValidation;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Constants;

namespace MovieAPI.Application.validators;

public class AdminUserUpdateValidator : AbstractValidator<AdminUserForUpdateDto>
{
  public AdminUserUpdateValidator()
  {
    RuleFor(u => u.Email).NotEmpty().EmailAddress();
    RuleFor(u => u.Role)
      .NotEmpty()
      .Must(Roles.All.Contains)
      .WithMessage($"Role must be one of: {string.Join(", ", Roles.All)}");
    RuleFor(u => u.DisplayName).MaximumLength(100);
  }
}
