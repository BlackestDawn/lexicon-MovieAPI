using FluentValidation;
using MovieAPI.Application.Models;
using MovieAPI.Domain.Constants;

namespace MovieAPI.Application.validators;

public class AdminUserCreationValidator : AbstractValidator<AdminUserForCreationDto>
{
  public AdminUserCreationValidator()
  {
    RuleFor(u => u.Email).NotEmpty().EmailAddress();
    RuleFor(u => u.Password).NotEmpty();
    RuleFor(u => u.Role)
      .NotEmpty()
      .Must(Roles.All.Contains)
      .WithMessage($"Role must be one of: {string.Join(", ", Roles.All)}");
    RuleFor(u => u.DisplayName).MaximumLength(100);
  }
}
