using FluentValidation;
using MovieAPI.Application.Models;

namespace MovieAPI.Application.validators;

public class ReviewUpdateValidator : AbstractValidator<ReviewForUpdateDto>
{
  public ReviewUpdateValidator()
  {
    RuleFor(r => r.AuthorName).NotEmpty();
    RuleFor(r => r.Body).NotEmpty();
    RuleFor(r => r.Score).GreaterThan(0).LessThanOrEqualTo(10);
  }
}
