using FluentValidation;
using MovieAPI.Application.Models;

namespace MovieAPI.Application.validators;

public class RegisterValidator : AbstractValidator<RegisterDto>
{
  public RegisterValidator()
  {
    RuleFor(r => r.Email).NotEmpty().EmailAddress();
    RuleFor(r => r.Password).NotEmpty();
  }
}
