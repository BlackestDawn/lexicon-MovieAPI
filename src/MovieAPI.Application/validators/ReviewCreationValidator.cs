using FluentValidation;
using MovieAPI.Application.Models;

namespace MovieAPI.Application.validators;

public class ReviewCreationValidator : AbstractValidator<ReviewForCreationDto>
{
  public ReviewCreationValidator()
  {
    RuleFor(r => r.AuthorName).NotEmpty();
    RuleFor(r => r.Body).NotEmpty();
    RuleFor(r => r.Score).GreaterThan(0).LessThanOrEqualTo(10);
  }
}
