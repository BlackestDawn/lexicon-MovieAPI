using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Moq;
using MovieAPI.Application.Models;
using MovieAPI.Application.Services;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;
using MovieAPI.Infrastructure.Models;
using MovieAPI.Infrastructure.Services;

namespace MovieAPI.UnitTests;

public class ReviewServiceTests
{
  private readonly Mock<IReviewRepository> _repo = new();
  private readonly Mock<IMovieRepository> _movieRepo = new();
  private readonly Mock<IMapper> _mapper = new();
  private readonly Mock<IValidator<ReviewForChangeDto>> _validator = new();
  private readonly ReviewService _sut;

  public ReviewServiceTests()
  {
    _sut = new ReviewService(_repo.Object, _movieRepo.Object, _mapper.Object, _validator.Object);
  }

  // Helpers

  private static ReviewForChangeDto MakeDto() => new()
  {
    AuthorName = "Roger Ebert",
    Body = "A masterpiece.",
    Score = 5
  };

  private static Review MakeReviewEntity(Guid? id = null, Guid? movieId = null) => new()
  {
    Id = id ?? Guid.NewGuid(),
    MovieId = movieId ?? Guid.NewGuid(),
    AuthorName = "Roger Ebert",
    Body = "A masterpiece.",
    Score = 5
  };

  private void SetupValidatorValid() =>
    _validator
      .Setup(v => v.Validate(It.IsAny<ReviewForChangeDto>()))
      .Returns(new ValidationResult());

  // Create

  [Fact]
  public async Task Create_WhenValidationFails_ReturnsFailed_WithValidationException()
  {
    _validator
      .Setup(v => v.Validate(It.IsAny<ReviewForChangeDto>()))
      .Returns(new ValidationResult([new ValidationFailure("AuthorName", "Required")]));

    var result = await _sut.Create(Guid.NewGuid(), MakeDto());

    Assert.False(result.Success);
    Assert.IsType<ValidationException>(result.Error);
  }

  [Fact]
  public async Task Create_WhenMovieNotFound_ReturnsFailed_WithMovieError()
  {
    var movieId = Guid.NewGuid();
    SetupValidatorValid();
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

    var result = await _sut.Create(movieId, MakeDto());

    Assert.False(result.Success);
    Assert.IsType<ArgumentException>(result.Error);
    Assert.Contains($"Movie '{movieId}' not found", result.Error!.Message);
  }

  [Fact]
  public async Task Create_WhenInputIsValid_ReturnsSuccessful()
  {
    var movieId = Guid.NewGuid();
    var dto = MakeDto();
    var entity = MakeReviewEntity(movieId: movieId);
    var reviewDto = new ReviewDto { Id = entity.Id, AuthorName = dto.AuthorName, Body = dto.Body, Score = dto.Score };

    SetupValidatorValid();
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _mapper.Setup(m => m.Map<Review>(dto)).Returns(entity);
    _mapper.Setup(m => m.Map<ReviewDto>(entity)).Returns(reviewDto);

    var result = await _sut.Create(movieId, dto);

    Assert.True(result.Success);
    Assert.Null(result.Error);
    Assert.Equal(entity.Id, result.Review!.Id);
  }

  [Fact]
  public async Task Create_WhenInputIsValid_CallsAddAndSaveExactlyOnce()
  {
    var movieId = Guid.NewGuid();
    var dto = MakeDto();
    var entity = MakeReviewEntity(movieId: movieId);

    SetupValidatorValid();
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _mapper.Setup(m => m.Map<Review>(dto)).Returns(entity);
    _mapper.Setup(m => m.Map<ReviewDto>(entity)).Returns(new ReviewDto { Id = entity.Id });

    await _sut.Create(movieId, dto);

    _repo.Verify(r => r.AddAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  // GetMany

  [Fact]
  public async Task GetMany_WhenPageAndSizeAreNull_UsesDefaults()
  {
    var movieId = Guid.NewGuid();
    var reviews = Enumerable.Empty<Review>();
    _repo
      .Setup(r => r.GetReviewsForMovieReadOnlyAsync(movieId, It.IsAny<ReviewSearchParams>(), 1, 10, It.IsAny<CancellationToken>()))
      .ReturnsAsync((reviews, null));
    _mapper.Setup(m => m.Map<IEnumerable<ReviewDto>>(reviews)).Returns([]);

    await _sut.GetMany(movieId, new ReviewSearchParams(null, null, null), null, null);

    _repo.Verify(r => r.GetReviewsForMovieReadOnlyAsync(movieId, It.IsAny<ReviewSearchParams>(), 1, 10, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetMany_WhenPageIsZeroAndSizeIsNegative_UsesDefaults()
  {
    var movieId = Guid.NewGuid();
    var reviews = Enumerable.Empty<Review>();
    _repo
      .Setup(r => r.GetReviewsForMovieReadOnlyAsync(movieId, It.IsAny<ReviewSearchParams>(), 1, 10, It.IsAny<CancellationToken>()))
      .ReturnsAsync((reviews, null));
    _mapper.Setup(m => m.Map<IEnumerable<ReviewDto>>(reviews)).Returns([]);

    await _sut.GetMany(movieId, new ReviewSearchParams(null, null, null), 0, -5);

    _repo.Verify(r => r.GetReviewsForMovieReadOnlyAsync(movieId, It.IsAny<ReviewSearchParams>(), 1, 10, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetMany_ReturnsMappedDtosAndPagination()
  {
    var movieId = Guid.NewGuid();
    var entity = MakeReviewEntity(movieId: movieId);
    var reviewDto = new ReviewDto { Id = entity.Id };
    var reviews = new[] { entity };
    var pagination = new PaginationMetadata(1, 10, 1);

    _repo
      .Setup(r => r.GetReviewsForMovieReadOnlyAsync(movieId, It.IsAny<ReviewSearchParams>(), 1, 10, It.IsAny<CancellationToken>()))
      .ReturnsAsync((reviews.AsEnumerable(), pagination));
    _mapper.Setup(m => m.Map<IEnumerable<ReviewDto>>(reviews.AsEnumerable())).Returns([reviewDto]);

    var (result, meta) = await _sut.GetMany(movieId, new ReviewSearchParams(null, null, null), null, null);

    Assert.Single(result);
    Assert.NotNull(meta);
  }

  // GetOne

  [Fact]
  public async Task GetOne_WhenNotFound_ReturnsNull()
  {
    var movieId = Guid.NewGuid();
    _repo
      .Setup(r => r.GetReviewReadOnlyAsync(movieId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((Review?)null);

    var result = await _sut.GetOne(movieId, Guid.NewGuid());

    Assert.Null(result);
  }

  [Fact]
  public async Task GetOne_WhenFound_ReturnsMappedDto()
  {
    var movieId = Guid.NewGuid();
    var entity = MakeReviewEntity(movieId: movieId);
    var dto = new ReviewDto { Id = entity.Id, AuthorName = entity.AuthorName, Body = entity.Body, Score = entity.Score };

    _repo.Setup(r => r.GetReviewReadOnlyAsync(movieId, entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _mapper.Setup(m => m.Map<ReviewDto>(entity)).Returns(dto);

    var result = await _sut.GetOne(movieId, entity.Id);

    Assert.NotNull(result);
    Assert.Equal(entity.Id, result.Id);
    Assert.Equal(entity.AuthorName, result.AuthorName);
  }

  [Fact]
  public async Task GetOne_WhenNotFound_DoesNotCallMapper()
  {
    var movieId = Guid.NewGuid();
    _repo
      .Setup(r => r.GetReviewReadOnlyAsync(movieId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((Review?)null);

    await _sut.GetOne(movieId, Guid.NewGuid());

    _mapper.Verify(m => m.Map<ReviewDto>(It.IsAny<Review>()), Times.Never);
  }

  // Remove

  [Fact]
  public async Task Remove_WhenNotFound_DoesNotDeleteOrSave()
  {
    var movieId = Guid.NewGuid();
    _repo
      .Setup(r => r.GetReviewAsync(movieId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((Review?)null);

    await _sut.Remove(movieId, Guid.NewGuid());

    _repo.Verify(r => r.Delete(It.IsAny<Review>()), Times.Never);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Remove_WhenFound_DeletesAndSaves()
  {
    var movieId = Guid.NewGuid();
    var entity = MakeReviewEntity(movieId: movieId);
    _repo.Setup(r => r.GetReviewAsync(movieId, entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    await _sut.Remove(movieId, entity.Id);

    _repo.Verify(r => r.Delete(entity), Times.Once);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  // Update (PUT)

  [Fact]
  public async Task UpdatePut_WhenMovieNotFound_ReturnsFalse()
  {
    var movieId = Guid.NewGuid();
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

    var (success, error) = await _sut.Update(movieId, Guid.NewGuid(), MakeDto());

    Assert.False(success);
    Assert.Contains($"Movie '{movieId}' not found", error);
  }

  [Fact]
  public async Task UpdatePut_WhenReviewNotFound_ReturnsFalse()
  {
    var movieId = Guid.NewGuid();
    var id = Guid.NewGuid();
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.GetReviewAsync(movieId, id, It.IsAny<CancellationToken>())).ReturnsAsync((Review?)null);

    var (success, error) = await _sut.Update(movieId, id, MakeDto());

    Assert.False(success);
    Assert.Contains($"Review '{id}' not found", error);
  }

  [Fact]
  public async Task UpdatePut_WhenValidationFails_ReturnsFalse()
  {
    var movieId = Guid.NewGuid();
    var entity = MakeReviewEntity(movieId: movieId);
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.GetReviewAsync(movieId, entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _validator
      .Setup(v => v.Validate(It.IsAny<ReviewForChangeDto>()))
      .Returns(new ValidationResult([new ValidationFailure("AuthorName", "Required")]));

    var (success, error) = await _sut.Update(movieId, entity.Id, MakeDto());

    Assert.False(success);
    Assert.NotNull(error);
  }

  [Fact]
  public async Task UpdatePut_WhenInputIsValid_ReturnsTrueAndSaves()
  {
    var movieId = Guid.NewGuid();
    var entity = MakeReviewEntity(movieId: movieId);

    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.GetReviewAsync(movieId, entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    SetupValidatorValid();
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    var dto = MakeDto();
    var (success, error) = await _sut.Update(movieId, entity.Id, dto);

    Assert.True(success);
    Assert.Null(error);
    Assert.Equal(dto.AuthorName, entity.AuthorName);
    Assert.Equal(dto.Body, entity.Body);
    Assert.Equal(dto.Score, entity.Score);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  // Update (PATCH)

  [Fact]
  public async Task UpdatePatch_WhenMovieNotFound_ReturnsFalse()
  {
    var movieId = Guid.NewGuid();
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

    var (success, error) = await _sut.Update(movieId, Guid.NewGuid(), new JsonPatchDocument<ReviewForChangeDto>());

    Assert.False(success);
    Assert.Contains($"Movie '{movieId}' not found", error);
  }

  [Fact]
  public async Task UpdatePatch_WhenReviewNotFound_ReturnsFalse()
  {
    var movieId = Guid.NewGuid();
    var id = Guid.NewGuid();
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.GetReviewAsync(movieId, id, It.IsAny<CancellationToken>())).ReturnsAsync((Review?)null);

    var (success, error) = await _sut.Update(movieId, id, new JsonPatchDocument<ReviewForChangeDto>());

    Assert.False(success);
    Assert.Contains($"Review '{id}' not found", error);
  }

  [Fact]
  public async Task UpdatePatch_WhenPatchIsValid_ReturnsTrueAndSaves()
  {
    var movieId = Guid.NewGuid();
    var entity = MakeReviewEntity(movieId: movieId);
    var updateDto = MakeDto();

    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.GetReviewAsync(movieId, entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _mapper.Setup(m => m.Map<ReviewForChangeDto>(entity)).Returns(updateDto);
    SetupValidatorValid();
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    var (success, error) = await _sut.Update(movieId, entity.Id, new JsonPatchDocument<ReviewForChangeDto>());

    Assert.True(success);
    Assert.Null(error);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }
}
