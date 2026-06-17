using MovieAPI.Application.Models;
using MovieAPI.Application.Validators;
using MovieAPI.Domain.Models;

namespace MovieAPI.UnitTests;

public class MovieUpdateValidatorTests
{
  private readonly MovieUpdateValidator _sut = new();

  private static MovieForUpdateDto ValidDto() => new()
  {
    Title = "Inception",
    Language = "English",
    PlotSummery = "A thief who enters the dreams of others.",
    Synopsis = "Dom Cobb is a skilled thief who steals secrets from deep within the subconscious.",
    RuntimeMinutes = 148,
    Budget = 160_000_000,
    ReleaseDate = new DateOnly(2010, 7, 16),
    Genres = [Guid.NewGuid()],
    CastCrews = [new CastCrewForCreationDto { PersonId = Guid.NewGuid(), Role = PersonRole.Director }]
  };

  [Fact]
  public void ValidDto_Passes()
  {
    Assert.True(_sut.Validate(ValidDto()).IsValid);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void EmptyTitle_Fails(string title)
  {
    var dto = ValidDto();
    dto.Title = title;
    Assert.False(_sut.Validate(dto).IsValid);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void EmptyLanguage_Fails(string language)
  {
    var dto = ValidDto();
    dto.Language = language;
    Assert.False(_sut.Validate(dto).IsValid);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void EmptyPlotSummery_Fails(string plot)
  {
    var dto = ValidDto();
    dto.PlotSummery = plot;
    Assert.False(_sut.Validate(dto).IsValid);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void EmptySynopsis_Fails(string synopsis)
  {
    var dto = ValidDto();
    dto.Synopsis = synopsis;
    Assert.False(_sut.Validate(dto).IsValid);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public void ZeroOrNegativeRuntime_FailsWithMessage(int runtime)
  {
    var dto = ValidDto();
    dto.RuntimeMinutes = runtime;
    var result = _sut.Validate(dto);
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.ErrorMessage == "Runtime must be positive");
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public void ZeroOrNegativeBudget_FailsWithMessage(int budget)
  {
    var dto = ValidDto();
    dto.Budget = budget;
    var result = _sut.Validate(dto);
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.ErrorMessage == "Budget must be positive");
  }

  [Fact]
  public void ReleaseDateBefore1850_Fails()
  {
    var dto = ValidDto();
    dto.ReleaseDate = new DateOnly(1849, 12, 31);
    Assert.False(_sut.Validate(dto).IsValid);
  }

  [Fact]
  public void ReleaseDateExactly1850_Passes()
  {
    var dto = ValidDto();
    dto.ReleaseDate = new DateOnly(1850, 1, 1);
    Assert.True(_sut.Validate(dto).IsValid);
  }

  [Fact]
  public void ReleaseDateMoreThan10YearsInFuture_Fails()
  {
    var dto = ValidDto();
    dto.ReleaseDate = DateOnly.FromDateTime(DateTime.Today.AddYears(10).AddDays(1));
    Assert.False(_sut.Validate(dto).IsValid);
  }

  [Fact]
  public void ReleaseDateExactly10YearsInFuture_Passes()
  {
    var dto = ValidDto();
    dto.ReleaseDate = DateOnly.FromDateTime(DateTime.Today.AddYears(10));
    Assert.True(_sut.Validate(dto).IsValid);
  }

  [Fact]
  public void EmptyGenres_FailsWithMessage()
  {
    var dto = ValidDto();
    dto.Genres = [];
    var result = _sut.Validate(dto);
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.ErrorMessage == "Must have at least 1 genre");
  }

  [Fact]
  public void EmptyCastCrews_FailsWithMessage()
  {
    var dto = ValidDto();
    dto.CastCrews = [];
    var result = _sut.Validate(dto);
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.ErrorMessage == "Must have at least 1 person for cast or crew");
  }
}
