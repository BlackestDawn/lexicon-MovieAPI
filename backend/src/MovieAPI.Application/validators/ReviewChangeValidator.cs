using FluentValidation;
using MovieAPI.Application.Models;

namespace MovieAPI.Application.validators;

public class ReviewChangeValidator : AbstractValidator<ReviewForChangeDto>
{
  public ReviewChangeValidator()
  {
    RuleFor(r => r.Body).NotEmpty();
    RuleFor(r => r.Score).GreaterThan(0).LessThanOrEqualTo(10);
  }
}
