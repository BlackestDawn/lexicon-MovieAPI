using AutoMapper;
using Moq;
using MovieAPI.Application.Exceptions;
using MovieAPI.Application.Models;
using MovieAPI.Application.Services;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.UnitTests;

public class FavoriteServiceTests
{
  private readonly Mock<IFavoriteRepository> _repo = new();
  private readonly Mock<IMovieRepository> _movieRepo = new();
  private readonly Mock<IMapper> _mapper = new();
  private readonly FavoriteService _sut;

  public FavoriteServiceTests()
  {
    _sut = new FavoriteService(_repo.Object, _movieRepo.Object, _mapper.Object);
  }

  private static Movie MakeMovieEntity(Guid? id = null) => new()
  {
    Id = id ?? Guid.NewGuid(),
    Title = "Inception",
  };

  // Exists

  [Fact]
  public async Task Exists_DelegatesToRepository()
  {
    var userId = Guid.NewGuid();
    var movieId = Guid.NewGuid();
    _repo.Setup(r => r.ExistsAsync(userId, movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

    var result = await _sut.Exists(movieId, userId);

    Assert.True(result);
  }

  // Add

  [Fact]
  public async Task Add_WhenMovieNotFound_ThrowsNotFoundException()
  {
    var movieId = Guid.NewGuid();
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

    var error = await Assert.ThrowsAsync<NotFoundException>(() => _sut.Add(movieId, Guid.NewGuid()));

    Assert.Contains($"Movie '{movieId}' not found", error.Message);
  }

  [Fact]
  public async Task Add_WhenAlreadyFavorited_DoesNotAddOrSaveAgain()
  {
    var movieId = Guid.NewGuid();
    var userId = Guid.NewGuid();
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.ExistsAsync(userId, movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

    await _sut.Add(movieId, userId);

    _repo.Verify(r => r.AddAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Add_WhenNotYetFavorited_AddsAndSaves()
  {
    var movieId = Guid.NewGuid();
    var userId = Guid.NewGuid();
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.ExistsAsync(userId, movieId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    await _sut.Add(movieId, userId);

    _repo.Verify(r => r.AddAsync(userId, movieId, It.IsAny<CancellationToken>()), Times.Once);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  // Remove

  [Fact]
  public async Task Remove_DelegatesToRepositoryAndSaves()
  {
    var movieId = Guid.NewGuid();
    var userId = Guid.NewGuid();
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    await _sut.Remove(movieId, userId);

    _repo.Verify(r => r.RemoveAsync(userId, movieId, It.IsAny<CancellationToken>()), Times.Once);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  // GetForUser

  [Fact]
  public async Task GetForUser_WhenPageAndSizeAreNull_UsesDefaults()
  {
    var userId = Guid.NewGuid();
    var movies = Enumerable.Empty<MovieListItem>();
    _repo
      .Setup(r => r.GetForUserReadOnlyAsync(userId, 1, 10, It.IsAny<CancellationToken>()))
      .ReturnsAsync((movies, null));

    await _sut.GetForUser(userId, null, null);

    _repo.Verify(r => r.GetForUserReadOnlyAsync(userId, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetForUser_WhenPageIsZeroAndSizeIsNegative_UsesDefaults()
  {
    var userId = Guid.NewGuid();
    var movies = Enumerable.Empty<MovieListItem>();
    _repo
      .Setup(r => r.GetForUserReadOnlyAsync(userId, 1, 10, It.IsAny<CancellationToken>()))
      .ReturnsAsync((movies, null));

    await _sut.GetForUser(userId, 0, -5);

    _repo.Verify(r => r.GetForUserReadOnlyAsync(userId, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetForUser_ReturnsMappedDtosWithAverageRatingAndPagination()
  {
    var userId = Guid.NewGuid();
    var entity = MakeMovieEntity();
    var movieDto = new MovieDto { Id = entity.Id };
    var movies = new[] { new MovieListItem(entity, 8.5m) };
    var pagination = new PaginationMetadata(1, 10, 1);

    _repo
      .Setup(r => r.GetForUserReadOnlyAsync(userId, 1, 10, It.IsAny<CancellationToken>()))
      .ReturnsAsync((movies.AsEnumerable(), pagination));
    _mapper.Setup(m => m.Map<MovieDto>(entity)).Returns(movieDto);

    var (result, meta) = await _sut.GetForUser(userId, null, null);

    var dto = Assert.Single(result);
    Assert.Equal(8.5m, dto.AverageRating);
    Assert.NotNull(meta);
  }
}
