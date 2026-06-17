using FluentValidation;
using MovieAPI.Application.Models;

namespace MovieAPI.Application.validators;

public class MovieUpdateValidator : AbstractValidator<MovieForUpdateDto>
{
  public MovieUpdateValidator()
  {
    RuleFor(m => m.Title).NotEmpty();
    RuleFor(m => m.Language).NotEmpty();
    RuleFor(m => m.PlotSummery).NotEmpty();
    RuleFor(m => m.Synopsis).NotEmpty();
    RuleFor(m => m.RuntimeMinutes).GreaterThan(0).WithMessage("Runtime must be positive");
    RuleFor(m => m.Budget).GreaterThan(0).WithMessage("Budget must be positive");
    RuleFor(m => m.ReleaseDate)
      .GreaterThanOrEqualTo(new DateOnly(1850, 1, 1))
      .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.Today.AddYears(10)));
    RuleFor(m => m.Genres).Must(mc => mc.Count > 0).WithMessage("Must have at least 1 genre");
    RuleFor(m => m.CastCrews).Must(mc => mc.Count > 0).WithMessage("Must have at least 1 person for cast or crew");
  }
}
