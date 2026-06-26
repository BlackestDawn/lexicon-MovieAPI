using MovieAPI.Application.Models;
using MovieAPI.Application.validators;

namespace MovieAPI.UnitTests;

public class PersonChangeValidatorTests
{
  private readonly PersonChangeValidator _sut = new();

  private static PersonForChangeDto ValidDto() => new()
  {
    GivenName = "Leonardo",
    LastName = "DiCaprio",
    DateOfBirth = new DateOnly(1974, 11, 11)
  };

  [Fact]
  public void ValidDto_Passes()
  {
    Assert.True(_sut.Validate(ValidDto()).IsValid);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void EmptyGivenName_Fails(string givenName)
  {
    var dto = ValidDto();
    dto.GivenName = givenName;
    Assert.False(_sut.Validate(dto).IsValid);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void EmptyLastName_Fails(string lastName)
  {
    var dto = ValidDto();
    dto.LastName = lastName;
    Assert.False(_sut.Validate(dto).IsValid);
  }

  [Fact]
  public void DateOfBirthBefore1750_Fails()
  {
    var dto = ValidDto();
    dto.DateOfBirth = new DateOnly(1749, 12, 31);
    Assert.False(_sut.Validate(dto).IsValid);
  }

  [Fact]
  public void DateOfBirthExactly1750_Passes()
  {
    var dto = ValidDto();
    dto.DateOfBirth = new DateOnly(1750, 1, 1);
    Assert.True(_sut.Validate(dto).IsValid);
  }

  [Fact]
  public void DateOfBirthInFuture_Fails()
  {
    var dto = ValidDto();
    dto.DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
    Assert.False(_sut.Validate(dto).IsValid);
  }

  [Fact]
  public void DateOfBirthToday_Passes()
  {
    var dto = ValidDto();
    dto.DateOfBirth = DateOnly.FromDateTime(DateTime.Today);
    Assert.True(_sut.Validate(dto).IsValid);
  }
}
