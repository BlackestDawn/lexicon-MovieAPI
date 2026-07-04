using FluentValidation;
using MovieAPI.Application.Models;

namespace MovieAPI.Application.validators;

public class LoginValidator : AbstractValidator<LoginDto>
{
  public LoginValidator()
  {
    RuleFor(l => l.Email).NotEmpty().EmailAddress();
    RuleFor(l => l.Password).NotEmpty();
  }
}
