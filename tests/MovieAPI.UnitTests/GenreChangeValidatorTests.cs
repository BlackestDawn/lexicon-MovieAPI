using MovieAPI.Application.Models;
using MovieAPI.Application.validators;

namespace MovieAPI.UnitTests;

public class GenreChangeValidatorTests
{
  private readonly GenreChangeValidator _sut = new();

  private static GenreForChangeDto ValidDto() => new()
  {
    Name = "Action",
    Slug = "action"
  };

  [Fact]
  public void ValidDto_Passes()
  {
    Assert.True(_sut.Validate(ValidDto()).IsValid);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void EmptyName_Fails(string name)
  {
    var dto = ValidDto();
    dto.Name = name;
    Assert.False(_sut.Validate(dto).IsValid);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void EmptySlug_Fails(string slug)
  {
    var dto = ValidDto();
    dto.Slug = slug;
    Assert.False(_sut.Validate(dto).IsValid);
  }
}
