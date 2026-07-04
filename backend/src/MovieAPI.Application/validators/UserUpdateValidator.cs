using FluentValidation;
using MovieAPI.Application.Models;

namespace MovieAPI.Application.validators;

public class UserUpdateValidator : AbstractValidator<UserForUpdateDto>
{
  public UserUpdateValidator()
  {
    RuleFor(u => u.Email).NotEmpty().EmailAddress();
  }
}
