using MovieAPI.Application.Models;
using MovieAPI.Application.validators;

namespace MovieAPI.UnitTests;

public class ReviewChangeValidatorTests
{
  private readonly ReviewChangeValidator _sut = new();

  private static ReviewForChangeDto ValidDto() => new()
  {
    Body = "A masterpiece.",
    Score = 5
  };

  [Fact]
  public void ValidDto_Passes()
  {
    Assert.True(_sut.Validate(ValidDto()).IsValid);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void EmptyBody_Fails(string body)
  {
    var dto = ValidDto();
    dto.Body = body;
    Assert.False(_sut.Validate(dto).IsValid);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public void ZeroOrNegativeScore_Fails(int score)
  {
    var dto = ValidDto();
    dto.Score = score;
    Assert.False(_sut.Validate(dto).IsValid);
  }

  [Fact]
  public void ScoreExactly1_Passes()
  {
    var dto = ValidDto();
    dto.Score = 1;
    Assert.True(_sut.Validate(dto).IsValid);
  }

  [Fact]
  public void ScoreExactly10_Passes()
  {
    var dto = ValidDto();
    dto.Score = 10;
    Assert.True(_sut.Validate(dto).IsValid);
  }

  [Fact]
  public void ScoreGreaterThan10_Fails()
  {
    var dto = ValidDto();
    dto.Score = 11;
    Assert.False(_sut.Validate(dto).IsValid);
  }
}
