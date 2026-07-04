using FluentValidation;
using MovieAPI.Application.Models;

namespace MovieAPI.Application.validators;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenDto>
{
  public RefreshTokenValidator()
  {
    RuleFor(r => r.RefreshToken).NotEmpty();
  }
}
