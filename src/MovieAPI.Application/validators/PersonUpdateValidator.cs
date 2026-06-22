using FluentValidation;
using MovieAPI.Application.Models;

namespace MovieAPI.Application.validators;

public class PersonUpdateValidator : AbstractValidator<PersonForUpdateDto>
{
  public PersonUpdateValidator()
  {
    RuleFor(p => p.FirstName).NotEmpty();
    RuleFor(p => p.LastName).NotEmpty();
    RuleFor(p => p.DateOfBirth)
      .GreaterThanOrEqualTo(new DateOnly(1750, 1, 1))
      .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.Today));
  }
}
