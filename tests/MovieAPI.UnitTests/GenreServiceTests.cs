using AutoMapper;
using Moq;
using MovieAPI.Application.Models;
using MovieAPI.Application.Services;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Services;

namespace MovieAPI.UnitTests;

public class GenreServiceTests
{
  private readonly Mock<IMovieRepository> _repo = new();
  private readonly Mock<IMapper> _mapper = new();
  private readonly GenreService _sut;

  public GenreServiceTests()
  {
    _sut = new GenreService(_repo.Object, _mapper.Object);
  }

  // Helpers

  private static Genre MakeGenreEntity(Guid? id = null) => new()
  {
    Id = id ?? Guid.NewGuid(),
    Name = "Action",
    Slug = "action"
  };

  private static GenreDto MakeGenreDto(Genre entity) => new()
  {
    Id = entity.Id,
    Name = entity.Name,
    Slug = entity.Slug
  };

  // GetMany

  [Fact]
  public async Task GetMany_WhenRepositoryReturnsEmpty_ReturnsEmptyCollection()
  {
    var genres = Enumerable.Empty<Genre>();
    _repo.Setup(r => r.GetGenresAsync(It.IsAny<CancellationToken>())).ReturnsAsync(genres);
    _mapper.Setup(m => m.Map<IEnumerable<GenreDto>>(genres)).Returns([]);

    var result = await _sut.GetMany(CancellationToken.None);

    Assert.Empty(result);
  }

  [Fact]
  public async Task GetMany_WhenRepositoryReturnsGenres_ReturnsMappedDtos()
  {
    var entity1 = MakeGenreEntity();
    var entity2 = MakeGenreEntity();
    var entities = new[] { entity1, entity2 };
    var dtos = new[] { MakeGenreDto(entity1), MakeGenreDto(entity2) };

    _repo.Setup(r => r.GetGenresAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entities.AsEnumerable());
    _mapper.Setup(m => m.Map<IEnumerable<GenreDto>>(entities.AsEnumerable())).Returns(dtos);

    var result = await _sut.GetMany(CancellationToken.None);

    Assert.Equal(2, result.Count());
  }

  [Fact]
  public async Task GetMany_CallsRepositoryExactlyOnce()
  {
    var genres = Enumerable.Empty<Genre>();
    _repo.Setup(r => r.GetGenresAsync(It.IsAny<CancellationToken>())).ReturnsAsync(genres);
    _mapper.Setup(m => m.Map<IEnumerable<GenreDto>>(genres)).Returns([]);

    await _sut.GetMany(CancellationToken.None);

    _repo.Verify(r => r.GetGenresAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  // GetOne

  [Fact]
  public async Task GetOne_WhenNotFound_ReturnsNull()
  {
    _repo
      .Setup(r => r.GetGenreAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((Genre?)null);

    var result = await _sut.GetOne(Guid.NewGuid(), CancellationToken.None);

    Assert.Null(result);
  }

  [Fact]
  public async Task GetOne_WhenFound_ReturnsMappedDto()
  {
    var entity = MakeGenreEntity();
    var dto = MakeGenreDto(entity);

    _repo.Setup(r => r.GetGenreAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _mapper.Setup(m => m.Map<GenreDto>(entity)).Returns(dto);

    var result = await _sut.GetOne(entity.Id, CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal(entity.Id, result.Id);
    Assert.Equal(entity.Name, result.Name);
    Assert.Equal(entity.Slug, result.Slug);
  }

  [Fact]
  public async Task GetOne_WhenNotFound_DoesNotCallMapper()
  {
    _repo
      .Setup(r => r.GetGenreAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((Genre?)null);

    await _sut.GetOne(Guid.NewGuid(), CancellationToken.None);

    _mapper.Verify(m => m.Map<GenreDto>(It.IsAny<Genre>()), Times.Never);
  }
}
