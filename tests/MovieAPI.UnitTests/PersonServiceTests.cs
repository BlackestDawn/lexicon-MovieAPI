using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Moq;
using MovieAPI.Application.Models;
using MovieAPI.Application.Services;
using MovieAPI.Domain.Entities;
using MovieAPI.Domain.Models;
using MovieAPI.Infrastructure.Interfaces;
using MovieAPI.Infrastructure.Models;
using MovieAPI.Infrastructure.Services;

namespace MovieAPI.UnitTests;

public class PersonServiceTests
{
  private readonly Mock<IPersonRepository> _repo = new();
  private readonly Mock<IMovieRepository> _movieRepo = new();
  private readonly Mock<IMapper> _mapper = new();
  private readonly Mock<IValidator<PersonForCreationDto>> _createValidator = new();
  private readonly Mock<IValidator<PersonForUpdateDto>> _updateValidator = new();
  private readonly PersonService _sut;

  public PersonServiceTests()
  {
    _sut = new PersonService(_repo.Object, _movieRepo.Object, _mapper.Object, _createValidator.Object, _updateValidator.Object);
  }

  // Helpers

  private static PersonForCreationDto MakeCreationDto(Guid? movieId = null) => new()
  {
    FirstName = "Leonardo",
    LastName = "DiCaprio",
    DateOfBirth = new DateOnly(1974, 11, 11),
    MovieRoles = [new MovieRoleForCreationDto { MovieId = movieId ?? Guid.NewGuid(), Role = PersonRole.Cast }]
  };

  private static PersonForUpdateDto MakeUpdateDto(Guid? movieId = null) => new()
  {
    FirstName = "Leonardo",
    LastName = "DiCaprio",
    DateOfBirth = new DateOnly(1974, 11, 11),
    MovieRoles = [new MovieRoleForCreationDto { MovieId = movieId ?? Guid.NewGuid(), Role = PersonRole.Cast }]
  };

  private static Person MakePersonEntity(Guid? id = null) => new()
  {
    Id = id ?? Guid.NewGuid(),
    FirstName = "Leonardo",
    LastName = "DiCaprio",
    DateOfBirth = new DateOnly(1974, 11, 11),
    CastCrews = []
  };

  private void SetupCreateValidatorValid() =>
    _createValidator
      .Setup(v => v.Validate(It.IsAny<PersonForCreationDto>()))
      .Returns(new ValidationResult());

  private void SetupUpdateValidatorValid() =>
    _updateValidator
      .Setup(v => v.Validate(It.IsAny<PersonForUpdateDto>()))
      .Returns(new ValidationResult());

  // Create

  [Fact]
  public async Task Create_WhenValidationFails_ReturnsFailed_WithValidationException()
  {
    _createValidator
      .Setup(v => v.Validate(It.IsAny<PersonForCreationDto>()))
      .Returns(new ValidationResult([new ValidationFailure("FirstName", "Required")]));

    var result = await _sut.Create(MakeCreationDto());

    Assert.False(result.Success);
    Assert.IsType<ValidationException>(result.Error);
  }

  [Fact]
  public async Task Create_WhenMovieIdNotFound_ReturnsFailed_WithMovieError()
  {
    var movieId = Guid.NewGuid();
    SetupCreateValidatorValid();
    _movieRepo
      .Setup(r => r.GetMissingIdsAsync(It.IsAny<ICollection<Guid>>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync([movieId]);

    var result = await _sut.Create(MakeCreationDto(movieId: movieId));

    Assert.False(result.Success);
    Assert.IsType<ArgumentException>(result.Error);
    Assert.Contains($"Movie '{movieId}' not found", result.Error!.Message);
  }

  [Fact]
  public async Task Create_WhenInputIsValid_ReturnsSuccessful()
  {
    var dto = MakeCreationDto();
    var entity = MakePersonEntity();
    var personDto = new PersonDto { Id = entity.Id, FirstName = dto.FirstName, LastName = dto.LastName };

    SetupCreateValidatorValid();
    _movieRepo
      .Setup(r => r.GetMissingIdsAsync(It.IsAny<ICollection<Guid>>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync([]);
    _repo.Setup(r => r.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _mapper.Setup(m => m.Map<Person>(It.IsAny<PersonForCreationDto>())).Returns(entity);
    _mapper.Setup(m => m.Map<ICollection<CastCrew>>(It.IsAny<ICollection<MovieRoleForCreationDto>>())).Returns([]);
    _mapper.Setup(m => m.Map<PersonDto>(It.IsAny<Person>())).Returns(personDto);

    var result = await _sut.Create(dto);

    Assert.True(result.Success);
    Assert.Null(result.Error);
    Assert.Equal(entity.Id, result.Person!.Id);
  }

  // GetMany

  [Fact]
  public async Task GetMany_WhenPageAndSizeAreNull_UsesDefaults()
  {
    var people = Enumerable.Empty<Person>();
    _repo
      .Setup(r => r.GetPeopleReadOnlyAsync(It.IsAny<PeopleSearchParams>(), 1, 10, It.IsAny<CancellationToken>()))
      .ReturnsAsync((people, null));
    _mapper.Setup(m => m.Map<IEnumerable<PersonDto>>(people)).Returns([]);

    await _sut.GetMany(new PeopleSearchParams(null, null, null), null, null);

    _repo.Verify(r => r.GetPeopleReadOnlyAsync(It.IsAny<PeopleSearchParams>(), 1, 10, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetMany_WhenPageIsZeroAndSizeIsNegative_UsesDefaults()
  {
    var people = Enumerable.Empty<Person>();
    _repo
      .Setup(r => r.GetPeopleReadOnlyAsync(It.IsAny<PeopleSearchParams>(), 1, 10, It.IsAny<CancellationToken>()))
      .ReturnsAsync((people, null));
    _mapper.Setup(m => m.Map<IEnumerable<PersonDto>>(people)).Returns([]);

    await _sut.GetMany(new PeopleSearchParams(null, null, null), 0, -5);

    _repo.Verify(r => r.GetPeopleReadOnlyAsync(It.IsAny<PeopleSearchParams>(), 1, 10, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetMany_ReturnsMappedDtosAndPagination()
  {
    var entity = MakePersonEntity();
    var personDto = new PersonDto { Id = entity.Id };
    var people = new[] { entity };
    var pagination = new PaginationMetadata(1, 10, 1);

    _repo
      .Setup(r => r.GetPeopleReadOnlyAsync(It.IsAny<PeopleSearchParams>(), 1, 10, It.IsAny<CancellationToken>()))
      .ReturnsAsync((people.AsEnumerable(), pagination));
    _mapper.Setup(m => m.Map<IEnumerable<PersonDto>>(people.AsEnumerable())).Returns([personDto]);

    var (result, meta) = await _sut.GetMany(new PeopleSearchParams(null, null, null), null, null);

    Assert.Single(result);
    Assert.NotNull(meta);
  }

  // GetOne

  [Fact]
  public async Task GetOne_WhenNotFound_ReturnsNull()
  {
    _repo
      .Setup(r => r.GetPersonReadOnlyAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((Person?)null);

    var result = await _sut.GetOne(Guid.NewGuid(), false);

    Assert.Null(result);
  }

  [Fact]
  public async Task GetOne_WhenFound_ReturnsMappedDto()
  {
    var entity = MakePersonEntity();
    var dto = new PersonExtendedDto { Id = entity.Id, FirstName = entity.FirstName };

    _repo.Setup(r => r.GetPersonReadOnlyAsync(entity.Id, false, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _mapper.Setup(m => m.Map<PersonExtendedDto>(entity)).Returns(dto);

    var result = await _sut.GetOne(entity.Id, false);

    Assert.NotNull(result);
    Assert.Equal(entity.Id, result.Id);
  }

  // Remove

  [Fact]
  public async Task Remove_WhenNotFound_DoesNotDeleteOrSave()
  {
    _repo
      .Setup(r => r.GetPersonAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
      .ReturnsAsync((Person?)null);

    await _sut.Remove(Guid.NewGuid());

    _repo.Verify(r => r.Delete(It.IsAny<Person>()), Times.Never);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Remove_WhenFound_DeletesAndSaves()
  {
    var entity = MakePersonEntity();
    _repo.Setup(r => r.GetPersonAsync(entity.Id, false, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    await _sut.Remove(entity.Id);

    _repo.Verify(r => r.Delete(entity), Times.Once);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  // Update (PUT)

  [Fact]
  public async Task UpdatePut_WhenPersonNotFound_ReturnsFalse()
  {
    var id = Guid.NewGuid();
    _repo.Setup(r => r.GetPersonAsync(id, true, It.IsAny<CancellationToken>())).ReturnsAsync((Person?)null);

    var (success, error) = await _sut.Update(id, MakeUpdateDto());

    Assert.False(success);
    Assert.Contains($"Person '{id}' not found", error);
  }

  [Fact]
  public async Task UpdatePut_WhenValidationFails_ReturnsFalse()
  {
    var entity = MakePersonEntity();
    _repo.Setup(r => r.GetPersonAsync(entity.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _updateValidator
      .Setup(v => v.Validate(It.IsAny<PersonForUpdateDto>()))
      .Returns(new ValidationResult([new ValidationFailure("FirstName", "Required")]));

    var (success, error) = await _sut.Update(entity.Id, MakeUpdateDto());

    Assert.False(success);
    Assert.NotNull(error);
  }

  [Fact]
  public async Task UpdatePut_WhenMovieIdNotFound_ReturnsFalse()
  {
    var movieId = Guid.NewGuid();
    var entity = MakePersonEntity();

    _repo.Setup(r => r.GetPersonAsync(entity.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    SetupUpdateValidatorValid();
    _movieRepo
      .Setup(r => r.GetMissingIdsAsync(It.IsAny<ICollection<Guid>>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync([movieId]);

    var (success, error) = await _sut.Update(entity.Id, MakeUpdateDto(movieId: movieId));

    Assert.False(success);
    Assert.Contains($"Movie '{movieId}' not found", error);
  }

  [Fact]
  public async Task UpdatePut_WhenInputIsValid_ReturnsTrueAndSaves()
  {
    var movieId = Guid.NewGuid();
    var entity = MakePersonEntity();

    _repo.Setup(r => r.GetPersonAsync(entity.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    SetupUpdateValidatorValid();
    _movieRepo
      .Setup(r => r.GetMissingIdsAsync(It.IsAny<ICollection<Guid>>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync([]);
    _mapper.Setup(m => m.Map<ICollection<CastCrew>>(It.IsAny<ICollection<MovieRoleForCreationDto>>())).Returns([]);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    var (success, error) = await _sut.Update(entity.Id, MakeUpdateDto(movieId: movieId));

    Assert.True(success);
    Assert.Null(error);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  // Update (PATCH)

  [Fact]
  public async Task UpdatePatch_WhenPersonNotFound_ReturnsFalse()
  {
    var id = Guid.NewGuid();
    _repo.Setup(r => r.GetPersonAsync(id, true, It.IsAny<CancellationToken>())).ReturnsAsync((Person?)null);

    var (success, error) = await _sut.Update(id, new JsonPatchDocument<PersonForUpdateDto>());

    Assert.False(success);
    Assert.Contains($"Person '{id}' not found", error);
  }

  [Fact]
  public async Task UpdatePatch_WhenPatchIsValid_ReturnsTrueAndSaves()
  {
    var movieId = Guid.NewGuid();
    var entity = MakePersonEntity();
    var updateDto = MakeUpdateDto(movieId: movieId);

    _repo.Setup(r => r.GetPersonAsync(entity.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _mapper.Setup(m => m.Map<PersonForUpdateDto>(entity)).Returns(updateDto);
    SetupUpdateValidatorValid();
    _movieRepo
      .Setup(r => r.GetMissingIdsAsync(It.IsAny<ICollection<Guid>>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync([]);
    _mapper.Setup(m => m.Map<ICollection<CastCrew>>(It.IsAny<ICollection<MovieRoleForCreationDto>>())).Returns([]);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    var (success, error) = await _sut.Update(entity.Id, new JsonPatchDocument<PersonForUpdateDto>());

    Assert.True(success);
    Assert.Null(error);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }
}
