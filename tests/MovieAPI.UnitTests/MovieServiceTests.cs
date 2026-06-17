using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Moq;
using MovieAPI.Application.Models;
using MovieAPI.Application.Services;
using MovieAPI.Domain.Entities;
using MovieAPI.Domain.Models;
using MovieAPI.Infrastructure.Models;
using MovieAPI.Infrastructure.Services;

namespace MovieAPI.UnitTests;

public class MovieServiceTests
{
  private readonly Mock<IMovieRepository> _repo = new();
  private readonly Mock<IMapper> _mapper = new();
  private readonly Mock<IValidator<MovieForCreationDto>> _createValidator = new();
  private readonly Mock<IValidator<MovieForUpdateDto>> _updateValidator = new();
  private readonly MovieService _sut;

  public MovieServiceTests()
  {
    _sut = new MovieService(_repo.Object, _mapper.Object, _createValidator.Object, _updateValidator.Object);
  }

  // ── Helpers ──────────────────────────────────────────────────────────────

  private static MovieForCreationDto MakeCreationDto(Guid? personId = null, Guid? genreId = null) => new()
  {
    Title = "Inception",
    Language = "English",
    PlotSummery = "A thief enters dreams.",
    Synopsis = "Full synopsis here.",
    RuntimeMinutes = 148,
    Budget = 160_000_000,
    ReleaseDate = new DateOnly(2010, 7, 16),
    Genres = [genreId ?? Guid.NewGuid()],
    CastCrews = [new CastCrewForCreationDto { PersonId = personId ?? Guid.NewGuid(), Role = PersonRole.Director }]
  };

  private static MovieForUpdateDto MakeUpdateDto(Guid? personId = null, Guid? genreId = null) => new()
  {
    Title = "Inception Updated",
    Language = "English",
    PlotSummery = "A thief enters dreams.",
    Synopsis = "Full synopsis here.",
    RuntimeMinutes = 148,
    Budget = 160_000_000,
    ReleaseDate = new DateOnly(2010, 7, 16),
    Genres = [genreId ?? Guid.NewGuid()],
    CastCrews = [new CastCrewForCreationDto { PersonId = personId ?? Guid.NewGuid(), Role = PersonRole.Director }]
  };

  private static Movie MakeMovieEntity(Guid? id = null) => new()
  {
    Id = id ?? Guid.NewGuid(),
    Title = "Inception",
    Details = new MovieDetail { Synopsis = "Synopsis", Language = "English", Budget = 1000 },
    CastCrews = [],
    MovieGenres = []
  };

  private void SetupCreateValidatorValid() =>
      _createValidator.Setup(v => v.Validate(It.IsAny<MovieForCreationDto>()))
          .Returns(new ValidationResult());

  private void SetupUpdateValidatorValid() =>
      _updateValidator.Setup(v => v.Validate(It.IsAny<MovieForUpdateDto>()))
          .Returns(new ValidationResult());

  // ── Create ───────────────────────────────────────────────────────────────

  [Fact]
  public async Task Create_WhenValidationFails_ReturnsFailed_WithValidationException()
  {
    _createValidator.Setup(v => v.Validate(It.IsAny<MovieForCreationDto>()))
        .Returns(new ValidationResult([new ValidationFailure("Title", "Required")]));

    var result = await _sut.Create(MakeCreationDto());

    Assert.False(result.Success);
    Assert.IsType<ValidationException>(result.Error);
  }

  [Fact]
  public async Task Create_WhenPersonIdNotFound_ReturnsFailed_WithPersonError()
  {
    var personId = Guid.NewGuid();
    SetupCreateValidatorValid();
    _repo.Setup(r => r.PersonExistsAsync(personId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
    _repo.Setup(r => r.GenreExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

    var result = await _sut.Create(MakeCreationDto(personId: personId));

    Assert.False(result.Success);
    Assert.IsType<ArgumentException>(result.Error);
    Assert.Contains($"Person '{personId}' not found", result.Error!.Message);
  }

  [Fact]
  public async Task Create_WhenGenreIdNotFound_ReturnsFailed_WithGenreError()
  {
    var genreId = Guid.NewGuid();
    SetupCreateValidatorValid();
    _repo.Setup(r => r.PersonExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.GenreExistsAsync(genreId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

    var result = await _sut.Create(MakeCreationDto(genreId: genreId));

    Assert.False(result.Success);
    Assert.IsType<ArgumentException>(result.Error);
    Assert.Contains($"Genre '{genreId}' not found", result.Error!.Message);
  }

  [Fact]
  public async Task Create_WhenInputIsValid_ReturnsSuccessful()
  {
    var dto = MakeCreationDto();
    var entity = MakeMovieEntity();
    var movieDto = new MovieDto { Id = entity.Id, Title = dto.Title };

    SetupCreateValidatorValid();
    _repo.Setup(r => r.PersonExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.GenreExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.AddMovieAsync(It.IsAny<Movie>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.GetMovieAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _mapper.Setup(m => m.Map<Movie>(It.IsAny<MovieForCreationDto>())).Returns(entity);
    _mapper.Setup(m => m.Map<MovieDto>(It.IsAny<Movie>())).Returns(movieDto);

    var result = await _sut.Create(dto);

    Assert.True(result.Success);
    Assert.Null(result.Error);
    Assert.Equal(entity.Id, result.Movie!.Id);
  }

  // ── GetMany ──────────────────────────────────────────────────────────────

  [Fact]
  public async Task GetMany_WhenPageAndSizeAreNull_UsesDefaults()
  {
    var movies = Enumerable.Empty<Movie>();
    _repo.Setup(r => r.GetMoviesReadOnlyAsync(It.IsAny<MovieSearchParams>(), 1, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync((movies, null));
    _mapper.Setup(m => m.Map<IEnumerable<MovieDto>>(movies)).Returns([]);

    await _sut.GetMany(new MovieSearchParams(null, null, null, null, null), null, null);

    _repo.Verify(r => r.GetMoviesReadOnlyAsync(It.IsAny<MovieSearchParams>(), 1, 10, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetMany_WhenPageIsZeroAndSizeIsNegative_UsesDefaults()
  {
    var movies = Enumerable.Empty<Movie>();
    _repo.Setup(r => r.GetMoviesReadOnlyAsync(It.IsAny<MovieSearchParams>(), 1, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync((movies, null));
    _mapper.Setup(m => m.Map<IEnumerable<MovieDto>>(movies)).Returns([]);

    await _sut.GetMany(new MovieSearchParams(null, null, null, null, null), 0, -5);

    _repo.Verify(r => r.GetMoviesReadOnlyAsync(It.IsAny<MovieSearchParams>(), 1, 10, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetMany_ReturnsMappedDtosAndPagination()
  {
    var entity = MakeMovieEntity();
    var movieDto = new MovieDto { Id = entity.Id };
    var movies = new[] { entity };
    var pagination = new PaginationMetadata(1, 10, 1);

    _repo.Setup(r => r.GetMoviesReadOnlyAsync(It.IsAny<MovieSearchParams>(), 1, 10, It.IsAny<CancellationToken>()))
        .ReturnsAsync((movies.AsEnumerable(), pagination));
    _mapper.Setup(m => m.Map<IEnumerable<MovieDto>>(movies.AsEnumerable())).Returns([movieDto]);

    var (result, meta) = await _sut.GetMany(new MovieSearchParams(null, null, null, null, null), null, null);

    Assert.Single(result);
    Assert.NotNull(meta);
  }

  // ── GetOne ───────────────────────────────────────────────────────────────

  [Fact]
  public async Task GetOne_WhenNotFound_ReturnsNull()
  {
    _repo.Setup(r => r.GetMovieAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
        .ReturnsAsync((Movie?)null);

    var result = await _sut.GetOne(Guid.NewGuid());

    Assert.Null(result);
  }

  [Fact]
  public async Task GetOne_WhenFound_ReturnsMappedDto()
  {
    var entity = MakeMovieEntity();
    var dto = new MovieExtendedDto { Id = entity.Id, Title = entity.Title };

    _repo.Setup(r => r.GetMovieAsync(entity.Id, false, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _mapper.Setup(m => m.Map<MovieExtendedDto>(entity)).Returns(dto);

    var result = await _sut.GetOne(entity.Id);

    Assert.NotNull(result);
    Assert.Equal(entity.Id, result.Id);
  }

  // ── Remove ───────────────────────────────────────────────────────────────

  [Fact]
  public async Task Remove_WhenNotFound_DoesNotDeleteOrSave()
  {
    _repo.Setup(r => r.GetMovieAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
        .ReturnsAsync((Movie?)null);

    await _sut.Remove(Guid.NewGuid());

    _repo.Verify(r => r.DeleteMovie(It.IsAny<Movie>()), Times.Never);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Remove_WhenFound_DeletesAndSaves()
  {
    var entity = MakeMovieEntity();
    _repo.Setup(r => r.GetMovieAsync(entity.Id, false, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    await _sut.Remove(entity.Id);

    _repo.Verify(r => r.DeleteMovie(entity), Times.Once);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  // ── Update (PUT) ─────────────────────────────────────────────────────────

  [Fact]
  public async Task UpdatePut_WhenMovieNotFound_ReturnsFalse()
  {
    var id = Guid.NewGuid();
    _repo.Setup(r => r.GetMovieAsync(id, true, It.IsAny<CancellationToken>())).ReturnsAsync((Movie?)null);

    var (success, error) = await _sut.Update(id, MakeUpdateDto());

    Assert.False(success);
    Assert.Contains($"Movie '{id}' not found", error);
  }

  [Fact]
  public async Task UpdatePut_WhenValidationFails_ReturnsFalse()
  {
    var entity = MakeMovieEntity();
    _repo.Setup(r => r.GetMovieAsync(entity.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _updateValidator.Setup(v => v.Validate(It.IsAny<MovieForUpdateDto>()))
        .Returns(new ValidationResult([new ValidationFailure("Title", "Required")]));

    var (success, error) = await _sut.Update(entity.Id, MakeUpdateDto());

    Assert.False(success);
    Assert.NotNull(error);
  }

  [Fact]
  public async Task UpdatePut_WhenPersonIdNotFound_ReturnsFalse()
  {
    var personId = Guid.NewGuid();
    var entity = MakeMovieEntity();

    _repo.Setup(r => r.GetMovieAsync(entity.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    SetupUpdateValidatorValid();
    _repo.Setup(r => r.PersonExistsAsync(personId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
    _repo.Setup(r => r.GenreExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

    var (success, error) = await _sut.Update(entity.Id, MakeUpdateDto(personId: personId));

    Assert.False(success);
    Assert.Contains($"Person '{personId}' not found", error);
  }

  [Fact]
  public async Task UpdatePut_WhenGenreIdNotFound_ReturnsFalse()
  {
    var genreId = Guid.NewGuid();
    var entity = MakeMovieEntity();

    _repo.Setup(r => r.GetMovieAsync(entity.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    SetupUpdateValidatorValid();
    _repo.Setup(r => r.PersonExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.GenreExistsAsync(genreId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

    var (success, error) = await _sut.Update(entity.Id, MakeUpdateDto(genreId: genreId));

    Assert.False(success);
    Assert.Contains($"Genre '{genreId}' not found", error);
  }

  [Fact]
  public async Task UpdatePut_WhenInputIsValid_ReturnsTrueAndSaves()
  {
    var personId = Guid.NewGuid();
    var genreId = Guid.NewGuid();
    var entity = MakeMovieEntity();

    _repo.Setup(r => r.GetMovieAsync(entity.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    SetupUpdateValidatorValid();
    _repo.Setup(r => r.PersonExistsAsync(personId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.GenreExistsAsync(genreId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    var (success, error) = await _sut.Update(entity.Id, MakeUpdateDto(personId: personId, genreId: genreId));

    Assert.True(success);
    Assert.Null(error);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  // ── Update (PATCH) ───────────────────────────────────────────────────────

  [Fact]
  public async Task UpdatePatch_WhenMovieNotFound_ReturnsFalse()
  {
    var id = Guid.NewGuid();
    _repo.Setup(r => r.GetMovieAsync(id, true, It.IsAny<CancellationToken>())).ReturnsAsync((Movie?)null);

    var (success, error) = await _sut.Update(id, new JsonPatchDocument<MovieForUpdateDto>());

    Assert.False(success);
    Assert.Contains($"Movie '{id}' not found", error);
  }

  [Fact]
  public async Task UpdatePatch_WhenPatchIsValid_ReturnsTrueAndSaves()
  {
    var personId = Guid.NewGuid();
    var genreId = Guid.NewGuid();
    var entity = MakeMovieEntity();
    var updateDto = MakeUpdateDto(personId: personId, genreId: genreId);

    _repo.Setup(r => r.GetMovieAsync(entity.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _mapper.Setup(m => m.Map<MovieForUpdateDto>(entity)).Returns(updateDto);
    SetupUpdateValidatorValid();
    _repo.Setup(r => r.PersonExistsAsync(personId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.GenreExistsAsync(genreId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    var (success, error) = await _sut.Update(entity.Id, new JsonPatchDocument<MovieForUpdateDto>());

    Assert.True(success);
    Assert.Null(error);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }
}
