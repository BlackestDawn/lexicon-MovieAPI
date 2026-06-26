using FluentValidation;
using MovieAPI.Application.Models;

namespace MovieAPI.Application.validators;

public class PersonChangeValidator : AbstractValidator<PersonForChangeDto>
{
  public PersonChangeValidator()
  {
    RuleFor(p => p.GivenName).NotEmpty();
    RuleFor(p => p.MiddleName).MaximumLength(50);
    RuleFor(p => p.LastName).NotEmpty();
    RuleFor(p => p.DateOfBirth)
      .GreaterThanOrEqualTo(new DateOnly(1750, 1, 1))
      .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.Today));
  }
}
