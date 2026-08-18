using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Moq;
using MovieAPI.Application.Exceptions;
using MovieAPI.Application.Models;
using MovieAPI.Application.Services;
using MovieAPI.Domain.Entities;
using MovieAPI.Domain.Models;
using MovieAPI.Infrastructure.Interfaces;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.UnitTests;

public class MovieServiceTests
{
  private readonly Mock<IMovieRepository> _repo = new();
  private readonly Mock<IPersonRepository> _personRepo = new();
  private readonly Mock<IGenreRepository> _genreRepo = new();
  private readonly Mock<IMapper> _mapper = new();
  private readonly Mock<IValidator<MovieForChangeDto>> _validator = new();
  private readonly MovieService _sut;

  public MovieServiceTests()
  {
    _sut = new MovieService(_repo.Object, _personRepo.Object, _genreRepo.Object, _mapper.Object, _validator.Object);
  }

  // Helpers

  private static MovieForChangeDto MakeDto(Guid? personId = null, Guid? genreId = null) => new()
  {
    Title = "Inception",
    Language = "English",
    PlotSummery = "A thief enters dreams.",
    Synopsis = "Full synopsis here.",
    RuntimeMinutes = 148,
    Budget = 160_000_000,
    ReleaseDate = new DateOnly(2010, 7, 16),
    Genres = [genreId ?? Guid.NewGuid()],
    CastCrews = [new CastCrewForChangeDto { PersonId = personId ?? Guid.NewGuid(), Role = PersonRole.Director }]
  };

  private static Movie MakeMovieEntity(Guid? id = null) => new()
  {
    Id = id ?? Guid.NewGuid(),
    Title = "Inception",
    Details = new MovieDetail { Synopsis = "Synopsis", Language = "English", Budget = 1000 },
    CastCrews = [],
    MovieGenres = []
  };

  private void SetupValidatorValid() =>
    _validator
      .Setup(v => v.ValidateAsync(It.IsAny<MovieForChangeDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult());

  // Create

  [Fact]
  public async Task Create_WhenValidationFails_ThrowsValidationException()
  {
    _validator
      .Setup(v => v.ValidateAsync(It.IsAny<MovieForChangeDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult([new ValidationFailure("Title", "Required")]));

    await Assert.ThrowsAsync<ValidationException>(() => _sut.Create(MakeDto()));
  }

  [Fact]
  public async Task Create_WhenPersonIdNotFound_ThrowsNotFoundException()
  {
    var personId = Guid.NewGuid();
    SetupValidatorValid();
    _personRepo.Setup(r => r.GetMissingIdsAsync(It.IsAny<ICollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([personId]);
    _genreRepo.Setup(r => r.GetMissingIdsAsync(It.IsAny<ICollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

    var error = await Assert.ThrowsAsync<NotFoundException>(() => _sut.Create(MakeDto(personId: personId)));

    Assert.Contains($"Person '{personId}' not found", error.Message);
  }

  [Fact]
  public async Task Create_WhenGenreIdNotFound_ThrowsNotFoundException()
  {
    var genreId = Guid.NewGuid();
    SetupValidatorValid();
    _personRepo.Setup(r => r.GetMissingIdsAsync(It.IsAny<ICollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
    _genreRepo.Setup(r => r.GetMissingIdsAsync(It.IsAny<ICollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([genreId]);

    var error = await Assert.ThrowsAsync<NotFoundException>(() => _sut.Create(MakeDto(genreId: genreId)));

    Assert.Contains($"Genre '{genreId}' not found", error.Message);
  }

  [Fact]
  public async Task Create_WhenInputIsValid_ReturnsMappedDto()
  {
    var dto = MakeDto();
    var entity = MakeMovieEntity();
    var movieDto = new MovieDto { Id = entity.Id, Title = dto.Title };

    SetupValidatorValid();
    _personRepo.Setup(r => r.GetMissingIdsAsync(It.IsAny<ICollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
    _genreRepo.Setup(r => r.GetMissingIdsAsync(It.IsAny<ICollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
    _repo.Setup(r => r.AddAsync(It.IsAny<Movie>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.GetMovieReadOnlyAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _mapper.Setup(m => m.Map<Movie>(It.IsAny<MovieForChangeDto>())).Returns(entity);
    _mapper.Setup(m => m.Map<MovieDto>(It.IsAny<Movie>())).Returns(movieDto);

    var result = await _sut.Create(dto);

    Assert.Equal(entity.Id, result.Id);
  }

  // GetMany

  [Fact]
  public async Task GetMany_WhenPageAndSizeAreNull_UsesDefaults()
  {
    var movies = Enumerable.Empty<MovieListItem>();
    _repo
      .Setup(r => r.GetMoviesReadOnlyAsync(It.IsAny<MovieSearchParams>(), 1, 10, It.IsAny<CancellationToken>()))
      .ReturnsAsync((movies, null));

    await _sut.GetMany(new MovieSearchParams(null, null, null, null, null, null), null, null);

    _repo.Verify(r => r.GetMoviesReadOnlyAsync(It.IsAny<MovieSearchParams>(), 1, 10, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetMany_WhenPageIsZeroAndSizeIsNegative_UsesDefaults()
  {
    var movies = Enumerable.Empty<MovieListItem>();
    _repo
      .Setup(r => r.GetMoviesReadOnlyAsync(It.IsAny<MovieSearchParams>(), 1, 10, It.IsAny<CancellationToken>()))
      .ReturnsAsync((movies, null));

    await _sut.GetMany(new MovieSearchParams(null, null, null, null, null, null), 0, -5);

    _repo.Verify(r => r.GetMoviesReadOnlyAsync(It.IsAny<MovieSearchParams>(), 1, 10, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetMany_ReturnsMappedDtosAndPagination()
  {
    var entity = MakeMovieEntity();
    var movieDto = new MovieDto { Id = entity.Id };
    var movies = new[] { new MovieListItem(entity, 8.5m) };
    var pagination = new PaginationMetadata(1, 10, 1);

    _repo
      .Setup(r => r.GetMoviesReadOnlyAsync(It.IsAny<MovieSearchParams>(), 1, 10, It.IsAny<CancellationToken>()))
      .ReturnsAsync((movies.AsEnumerable(), pagination));
    _mapper.Setup(m => m.Map<MovieDto>(entity)).Returns(movieDto);

    var (result, meta) = await _sut.GetMany(new MovieSearchParams(null, null, null, null, null, null), null, null);

    var dto = Assert.Single(result);
    Assert.Equal(8.5m, dto.AverageRating);
    Assert.NotNull(meta);
  }

  // GetOne

  [Fact]
  public async Task GetOne_WhenNotFound_ThrowsNotFoundException()
  {
    _repo
      .Setup(r => r.GetMovieReadOnlyAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
      .ReturnsAsync((Movie?)null);

    await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetOne(Guid.NewGuid()));
  }

  [Fact]
  public async Task GetOne_WhenFound_ReturnsMappedDto()
  {
    var entity = MakeMovieEntity();
    var dto = new MovieExtendedDto { Id = entity.Id, Title = entity.Title };

    _repo.Setup(r => r.GetMovieReadOnlyAsync(entity.Id, false, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _mapper.Setup(m => m.Map<MovieExtendedDto>(entity)).Returns(dto);

    var result = await _sut.GetOne(entity.Id);

    Assert.NotNull(result);
    Assert.Equal(entity.Id, result.Id);
  }

  // Remove

  [Fact]
  public async Task Remove_WhenNotFound_DoesNotDeleteOrSave()
  {
    _repo
      .Setup(r => r.GetMovieAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
      .ReturnsAsync((Movie?)null);

    await _sut.Remove(Guid.NewGuid());

    _repo.Verify(r => r.Delete(It.IsAny<Movie>()), Times.Never);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Remove_WhenFound_DeletesAndSaves()
  {
    var entity = MakeMovieEntity();
    _repo.Setup(r => r.GetMovieAsync(entity.Id, false, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    await _sut.Remove(entity.Id);

    _repo.Verify(r => r.Delete(entity), Times.Once);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  // Update (PUT)

  [Fact]
  public async Task UpdatePut_WhenMovieNotFound_ThrowsNotFoundException()
  {
    var id = Guid.NewGuid();
    _repo.Setup(r => r.GetMovieAsync(id, true, It.IsAny<CancellationToken>())).ReturnsAsync((Movie?)null);

    var error = await Assert.ThrowsAsync<NotFoundException>(() => _sut.Update(id, MakeDto()));

    Assert.Contains($"Movie '{id}' not found", error.Message);
  }

  [Fact]
  public async Task UpdatePut_WhenValidationFails_ThrowsValidationException()
  {
    var entity = MakeMovieEntity();
    _repo.Setup(r => r.GetMovieAsync(entity.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _validator
      .Setup(v => v.ValidateAsync(It.IsAny<MovieForChangeDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult([new ValidationFailure("Title", "Required")]));

    await Assert.ThrowsAsync<ValidationException>(() => _sut.Update(entity.Id, MakeDto()));
  }

  [Fact]
  public async Task UpdatePut_WhenPersonIdNotFound_ThrowsNotFoundException()
  {
    var personId = Guid.NewGuid();
    var entity = MakeMovieEntity();

    _repo.Setup(r => r.GetMovieAsync(entity.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    SetupValidatorValid();
    _personRepo.Setup(r => r.GetMissingIdsAsync(It.IsAny<ICollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([personId]);
    _genreRepo.Setup(r => r.GetMissingIdsAsync(It.IsAny<ICollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

    var error = await Assert.ThrowsAsync<NotFoundException>(() => _sut.Update(entity.Id, MakeDto(personId: personId)));

    Assert.Contains($"Person '{personId}' not found", error.Message);
  }

  [Fact]
  public async Task UpdatePut_WhenGenreIdNotFound_ThrowsNotFoundException()
  {
    var genreId = Guid.NewGuid();
    var entity = MakeMovieEntity();

    _repo.Setup(r => r.GetMovieAsync(entity.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    SetupValidatorValid();
    _personRepo.Setup(r => r.GetMissingIdsAsync(It.IsAny<ICollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
    _genreRepo.Setup(r => r.GetMissingIdsAsync(It.IsAny<ICollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([genreId]);

    var error = await Assert.ThrowsAsync<NotFoundException>(() => _sut.Update(entity.Id, MakeDto(genreId: genreId)));

    Assert.Contains($"Genre '{genreId}' not found", error.Message);
  }

  [Fact]
  public async Task UpdatePut_WhenInputIsValid_Saves()
  {
    var personId = Guid.NewGuid();
    var genreId = Guid.NewGuid();
    var entity = MakeMovieEntity();

    _repo.Setup(r => r.GetMovieAsync(entity.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    SetupValidatorValid();
    _personRepo.Setup(r => r.GetMissingIdsAsync(It.IsAny<ICollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
    _genreRepo.Setup(r => r.GetMissingIdsAsync(It.IsAny<ICollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
    _mapper.Setup(m => m.Map<ICollection<CastCrew>>(It.IsAny<ICollection<CastCrewForChangeDto>>())).Returns([]);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    await _sut.Update(entity.Id, MakeDto(personId: personId, genreId: genreId));

    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  // Update (PATCH)

  [Fact]
  public async Task UpdatePatch_WhenMovieNotFound_ThrowsNotFoundException()
  {
    var id = Guid.NewGuid();
    _repo.Setup(r => r.GetMovieAsync(id, true, It.IsAny<CancellationToken>())).ReturnsAsync((Movie?)null);

    var error = await Assert.ThrowsAsync<NotFoundException>(() => _sut.Update(id, new JsonPatchDocument<MovieForChangeDto>()));

    Assert.Contains($"Movie '{id}' not found", error.Message);
  }

  [Fact]
  public async Task UpdatePatch_WhenPatchIsValid_Saves()
  {
    var personId = Guid.NewGuid();
    var genreId = Guid.NewGuid();
    var entity = MakeMovieEntity();
    var updateDto = MakeDto(personId: personId, genreId: genreId);

    _repo.Setup(r => r.GetMovieAsync(entity.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _mapper.Setup(m => m.Map<MovieForChangeDto>(entity)).Returns(updateDto);
    SetupValidatorValid();
    _personRepo.Setup(r => r.GetMissingIdsAsync(It.IsAny<ICollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
    _genreRepo.Setup(r => r.GetMissingIdsAsync(It.IsAny<ICollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
    _mapper.Setup(m => m.Map<ICollection<CastCrew>>(It.IsAny<ICollection<CastCrewForChangeDto>>())).Returns([]);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    await _sut.Update(entity.Id, new JsonPatchDocument<MovieForChangeDto>());

    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }
}
