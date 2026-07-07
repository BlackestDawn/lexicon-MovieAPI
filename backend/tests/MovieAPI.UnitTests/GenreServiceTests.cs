using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Moq;
using MovieAPI.Application.Exceptions;
using MovieAPI.Application.Models;
using MovieAPI.Application.Services;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;

namespace MovieAPI.UnitTests;

public class GenreServiceTests
{
  private readonly Mock<IGenreRepository> _repo = new();
  private readonly Mock<IMovieRepository> _movieRepo = new();
  private readonly Mock<IMapper> _mapper = new();
  private readonly Mock<IValidator<GenreForChangeDto>> _validator = new();
  private readonly GenreService _sut;

  public GenreServiceTests()
  {
    _sut = new GenreService(_repo.Object, _movieRepo.Object, _mapper.Object, _validator.Object);
  }

  // Helpers

  private static GenreForChangeDto MakeDto() => new()
  {
    Name = "Action",
    Slug = "action"
  };

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

  private static GenreExtendedDto MakeGenreExtendedDto(Genre entity) => new()
  {
    Id = entity.Id,
    Name = entity.Name,
    Slug = entity.Slug
  };

  private void SetupValidatorValid() =>
    _validator
      .Setup(v => v.ValidateAsync(It.IsAny<GenreForChangeDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult());

  // Create

  [Fact]
  public async Task Create_WhenValidationFails_ThrowsValidationException()
  {
    _validator
      .Setup(v => v.ValidateAsync(It.IsAny<GenreForChangeDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult([new ValidationFailure("Name", "Required")]));

    await Assert.ThrowsAsync<ValidationException>(() => _sut.Create(MakeDto(), CancellationToken.None));
  }

  [Fact]
  public async Task Create_WhenInputIsValid_ReturnsMappedDto()
  {
    var dto = MakeDto();
    var entity = MakeGenreEntity();
    var genreDto = MakeGenreDto(entity);

    SetupValidatorValid();
    _repo.Setup(r => r.AddAsync(It.IsAny<Genre>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _mapper.Setup(m => m.Map<Genre>(It.IsAny<GenreForChangeDto>())).Returns(entity);
    _mapper.Setup(m => m.Map<GenreDto>(It.IsAny<Genre>())).Returns(genreDto);

    var result = await _sut.Create(dto, CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal(entity.Id, result.Id);
  }

  [Fact]
  public async Task Create_WhenInputIsValid_CallsAddAndSaveExactlyOnce()
  {
    var dto = MakeDto();
    var entity = MakeGenreEntity();

    SetupValidatorValid();
    _repo.Setup(r => r.AddAsync(It.IsAny<Genre>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _mapper.Setup(m => m.Map<Genre>(It.IsAny<GenreForChangeDto>())).Returns(entity);
    _mapper.Setup(m => m.Map<GenreDto>(It.IsAny<Genre>())).Returns(MakeGenreDto(entity));

    await _sut.Create(dto, CancellationToken.None);

    _repo.Verify(r => r.AddAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  // GetMany

  [Fact]
  public async Task GetMany_WhenRepositoryReturnsEmpty_ReturnsEmptyCollection()
  {
    var genres = Enumerable.Empty<Genre>();
    _repo.Setup(r => r.GetGenresReadOnlyAsync(It.IsAny<CancellationToken>())).ReturnsAsync(genres);
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

    _repo.Setup(r => r.GetGenresReadOnlyAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entities.AsEnumerable());
    _mapper.Setup(m => m.Map<IEnumerable<GenreDto>>(entities.AsEnumerable())).Returns(dtos);

    var result = await _sut.GetMany(CancellationToken.None);

    Assert.Equal(2, result.Count());
  }

  [Fact]
  public async Task GetMany_CallsRepositoryExactlyOnce()
  {
    var genres = Enumerable.Empty<Genre>();
    _repo.Setup(r => r.GetGenresReadOnlyAsync(It.IsAny<CancellationToken>())).ReturnsAsync(genres);
    _mapper.Setup(m => m.Map<IEnumerable<GenreDto>>(genres)).Returns([]);

    await _sut.GetMany(CancellationToken.None);

    _repo.Verify(r => r.GetGenresReadOnlyAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  // GetOne

  [Fact]
  public async Task GetOne_WhenNotFound_ThrowsNotFoundException()
  {
    _repo
      .Setup(r => r.GetGenreReadOnlyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((Genre?)null);

    await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetOne(Guid.NewGuid(), false, CancellationToken.None));
  }

  [Fact]
  public async Task GetOne_WhenFound_ReturnsMappedDto()
  {
    var entity = MakeGenreEntity();
    var dto = MakeGenreExtendedDto(entity);

    _repo.Setup(r => r.GetGenreReadOnlyAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _mapper.Setup(m => m.Map<GenreExtendedDto>(entity)).Returns(dto);

    var result = await _sut.GetOne(entity.Id, false, CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal(entity.Id, result.Id);
    Assert.Equal(entity.Name, result.Name);
    Assert.Equal(entity.Slug, result.Slug);
  }

  [Fact]
  public async Task GetOne_WhenNotFound_DoesNotCallMapper()
  {
    _repo
      .Setup(r => r.GetGenreReadOnlyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((Genre?)null);

    await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetOne(Guid.NewGuid(), false, CancellationToken.None));

    _mapper.Verify(m => m.Map<GenreExtendedDto>(It.IsAny<Genre>()), Times.Never);
  }

  // Remove

  [Fact]
  public async Task Remove_WhenNotFound_DoesNotDeleteOrSave()
  {
    _repo
      .Setup(r => r.GetGenreAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((Genre?)null);

    await _sut.Remove(Guid.NewGuid(), CancellationToken.None);

    _repo.Verify(r => r.Delete(It.IsAny<Genre>()), Times.Never);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Remove_WhenFound_DeletesAndSaves()
  {
    var entity = MakeGenreEntity();
    _repo.Setup(r => r.GetGenreAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    await _sut.Remove(entity.Id, CancellationToken.None);

    _repo.Verify(r => r.Delete(entity), Times.Once);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  // Update (PUT)

  [Fact]
  public async Task UpdatePut_WhenGenreNotFound_ThrowsNotFoundException()
  {
    var id = Guid.NewGuid();
    _repo.Setup(r => r.GetGenreAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Genre?)null);

    var error = await Assert.ThrowsAsync<NotFoundException>(() => _sut.Update(id, MakeDto(), CancellationToken.None));

    Assert.Contains($"Genre '{id}' not found", error.Message);
  }

  [Fact]
  public async Task UpdatePut_WhenValidationFails_ThrowsValidationException()
  {
    var entity = MakeGenreEntity();
    _repo.Setup(r => r.GetGenreAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _validator
      .Setup(v => v.ValidateAsync(It.IsAny<GenreForChangeDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult([new ValidationFailure("Name", "Required")]));

    await Assert.ThrowsAsync<ValidationException>(() => _sut.Update(entity.Id, MakeDto(), CancellationToken.None));
  }

  [Fact]
  public async Task UpdatePut_WhenInputIsValid_Saves()
  {
    var entity = MakeGenreEntity();
    var dto = new GenreForChangeDto { Name = "Comedy", Slug = "comedy" };

    _repo.Setup(r => r.GetGenreAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    SetupValidatorValid();
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    await _sut.Update(entity.Id, dto, CancellationToken.None);

    Assert.Equal(dto.Name, entity.Name);
    Assert.Equal(dto.Slug, entity.Slug);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  // Update (PATCH)

  [Fact]
  public async Task UpdatePatch_WhenGenreNotFound_ThrowsNotFoundException()
  {
    var id = Guid.NewGuid();
    _repo.Setup(r => r.GetGenreAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Genre?)null);

    var error = await Assert.ThrowsAsync<NotFoundException>(() => _sut.Update(id, new JsonPatchDocument<GenreForChangeDto>(), CancellationToken.None));

    Assert.Contains($"Genre '{id}' not found", error.Message);
  }

  [Fact]
  public async Task UpdatePatch_WhenPatchIsValid_Saves()
  {
    var entity = MakeGenreEntity();
    var updateDto = MakeDto();

    _repo.Setup(r => r.GetGenreAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _mapper.Setup(m => m.Map<GenreForChangeDto>(entity)).Returns(updateDto);
    SetupValidatorValid();
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    await _sut.Update(entity.Id, new JsonPatchDocument<GenreForChangeDto>(), CancellationToken.None);

    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }
}
