using FluentValidation;
using MovieAPI.Application.Models;

namespace MovieAPI.Application.validators;

public class GenreChangeValidator : AbstractValidator<GenreForChangeDto>
{
  public GenreChangeValidator()
  {
    RuleFor(g => g.Name).NotEmpty();
    RuleFor(g => g.Slug).NotEmpty();
  }
}
