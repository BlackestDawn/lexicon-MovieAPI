using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Moq;
using MovieAPI.Application.Exceptions;
using MovieAPI.Application.Models;
using MovieAPI.Application.Services;
using MovieAPI.Domain.Entities;
using MovieAPI.Infrastructure.Interfaces;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.UnitTests;

public class ReviewServiceTests
{
  private readonly Mock<IReviewRepository> _repo = new();
  private readonly Mock<IMovieRepository> _movieRepo = new();
  private readonly Mock<UserManager<ApplicationUser>> _userManager;
  private readonly Mock<IMapper> _mapper = new();
  private readonly Mock<IValidator<ReviewForChangeDto>> _validator = new();
  private readonly ReviewService _sut;

  public ReviewServiceTests()
  {
    _userManager = IdentityMocks.MockUserManager();
    // Default author lookup for tests that don't care about the resolved name
    // specifically (e.g. Create tests) - matches MakeReviewEntity's AuthorName below.
    _userManager
      .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(new ApplicationUser { DisplayName = "Roger Ebert" });
    _sut = new ReviewService(_repo.Object, _movieRepo.Object, _userManager.Object, _mapper.Object, _validator.Object);
  }

  // Helpers

  private static ReviewForChangeDto MakeDto() => new()
  {
    Body = "A masterpiece.",
    Score = 5
  };

  private static Review MakeReviewEntity(Guid? id = null, Guid? movieId = null, Guid? userId = null) => new()
  {
    Id = id ?? Guid.NewGuid(),
    MovieId = movieId ?? Guid.NewGuid(),
    UserId = userId ?? Guid.NewGuid(),
    AuthorName = "Roger Ebert",
    Body = "A masterpiece.",
    Score = 5
  };

  private void SetupValidatorValid() =>
    _validator
      .Setup(v => v.ValidateAsync(It.IsAny<ReviewForChangeDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult());

  // Create

  [Fact]
  public async Task Create_WhenValidationFails_ThrowsValidationException()
  {
    var movieId = Guid.NewGuid();

    _validator
      .Setup(v => v.ValidateAsync(It.IsAny<ReviewForChangeDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult([new ValidationFailure("Body", "Required")]));
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

    await Assert.ThrowsAsync<ValidationException>(() => _sut.Create(movieId, MakeDto(), Guid.NewGuid()));
  }

  [Fact]
  public async Task Create_WhenMovieNotFound_ThrowsNotFoundException()
  {
    var movieId = Guid.NewGuid();
    SetupValidatorValid();
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

    var error = await Assert.ThrowsAsync<NotFoundException>(() => _sut.Create(movieId, MakeDto(), Guid.NewGuid()));

    Assert.Contains($"Movie '{movieId}' not found", error.Message);
  }

  [Fact]
  public async Task Create_WhenInputIsValid_ReturnsMappedDto()
  {
    var movieId = Guid.NewGuid();
    var currentUserId = Guid.NewGuid();
    var dto = MakeDto();
    var entity = MakeReviewEntity(movieId: movieId);
    var reviewDto = new ReviewDto { Id = entity.Id, AuthorName = entity.AuthorName, Body = dto.Body, Score = dto.Score };

    SetupValidatorValid();
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _mapper.Setup(m => m.Map<Review>(dto)).Returns(entity);
    _mapper.Setup(m => m.Map<ReviewDto>(entity)).Returns(reviewDto);

    var result = await _sut.Create(movieId, dto, currentUserId);

    Assert.NotNull(result);
    Assert.Equal(entity.AuthorName, result.AuthorName);
    Assert.Equal(entity.Body, result.Body);
    Assert.Equal(entity.Id, result.Id);
  }

  [Fact]
  public async Task Create_WhenInputIsValid_SetsUserIdToCurrentUser()
  {
    var movieId = Guid.NewGuid();
    var currentUserId = Guid.NewGuid();
    var dto = MakeDto();
    var entity = MakeReviewEntity(movieId: movieId);

    SetupValidatorValid();
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _mapper.Setup(m => m.Map<Review>(dto)).Returns(entity);
    _mapper.Setup(m => m.Map<ReviewDto>(entity)).Returns(new ReviewDto { Id = entity.Id });

    await _sut.Create(movieId, dto, currentUserId);

    Assert.Equal(currentUserId, entity.UserId);
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

    await _sut.Create(movieId, dto, Guid.NewGuid());

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
  public async Task GetOne_WhenNotFound_ThrowsNotFoundException()
  {
    var movieId = Guid.NewGuid();
    _repo
      .Setup(r => r.GetReviewReadOnlyAsync(movieId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((Review?)null);

    await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetOne(movieId, Guid.NewGuid()));
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

    await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetOne(movieId, Guid.NewGuid()));

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

    await _sut.Remove(movieId, Guid.NewGuid(), Guid.NewGuid(), canModerate: true);

    _repo.Verify(r => r.Delete(It.IsAny<Review>()), Times.Never);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Remove_WhenOwner_DeletesAndSaves()
  {
    var movieId = Guid.NewGuid();
    var ownerId = Guid.NewGuid();
    var entity = MakeReviewEntity(movieId: movieId, userId: ownerId);
    _repo.Setup(r => r.GetReviewAsync(movieId, entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    await _sut.Remove(movieId, entity.Id, ownerId, canModerate: false);

    _repo.Verify(r => r.Delete(entity), Times.Once);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task Remove_WhenNotOwnerAndCannotModerate_ThrowsForbiddenException()
  {
    var movieId = Guid.NewGuid();
    var entity = MakeReviewEntity(movieId: movieId, userId: Guid.NewGuid());
    _repo.Setup(r => r.GetReviewAsync(movieId, entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

    await Assert.ThrowsAsync<ForbiddenException>(() => _sut.Remove(movieId, entity.Id, Guid.NewGuid(), canModerate: false));

    _repo.Verify(r => r.Delete(It.IsAny<Review>()), Times.Never);
  }

  [Fact]
  public async Task Remove_WhenNotOwnerButCanModerate_DeletesAndSaves()
  {
    var movieId = Guid.NewGuid();
    var entity = MakeReviewEntity(movieId: movieId, userId: Guid.NewGuid());
    _repo.Setup(r => r.GetReviewAsync(movieId, entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    await _sut.Remove(movieId, entity.Id, Guid.NewGuid(), canModerate: true);

    _repo.Verify(r => r.Delete(entity), Times.Once);
  }

  // Update (PUT)

  [Fact]
  public async Task UpdatePut_WhenMovieNotFound_ThrowsNotFoundException()
  {
    var movieId = Guid.NewGuid();
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

    var error = await Assert.ThrowsAsync<NotFoundException>(() => _sut.Update(movieId, Guid.NewGuid(), MakeDto(), Guid.NewGuid(), canModerate: true));

    Assert.Contains($"Movie '{movieId}' not found", error.Message);
  }

  [Fact]
  public async Task UpdatePut_WhenReviewNotFound_ThrowsNotFoundException()
  {
    var movieId = Guid.NewGuid();
    var id = Guid.NewGuid();
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.GetReviewAsync(movieId, id, It.IsAny<CancellationToken>())).ReturnsAsync((Review?)null);

    var error = await Assert.ThrowsAsync<NotFoundException>(() => _sut.Update(movieId, id, MakeDto(), Guid.NewGuid(), canModerate: true));

    Assert.Contains($"Review '{id}' not found", error.Message);
  }

  [Fact]
  public async Task UpdatePut_WhenNotOwnerAndCannotModerate_ThrowsForbiddenException()
  {
    var movieId = Guid.NewGuid();
    var entity = MakeReviewEntity(movieId: movieId, userId: Guid.NewGuid());
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.GetReviewAsync(movieId, entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

    await Assert.ThrowsAsync<ForbiddenException>(
      () => _sut.Update(movieId, entity.Id, MakeDto(), Guid.NewGuid(), canModerate: false));

    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task UpdatePut_WhenValidationFails_ThrowsValidationError()
  {
    var movieId = Guid.NewGuid();
    var ownerId = Guid.NewGuid();
    var entity = MakeReviewEntity(movieId: movieId, userId: ownerId);
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.GetReviewAsync(movieId, entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _validator
      .Setup(v => v.ValidateAsync(It.IsAny<ReviewForChangeDto>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult([new ValidationFailure("Body", "Required")]));

    await Assert.ThrowsAsync<ValidationException>(() => _sut.Update(movieId, entity.Id, MakeDto(), ownerId, canModerate: false));
  }

  [Fact]
  public async Task UpdatePut_WhenOwner_Saves()
  {
    var movieId = Guid.NewGuid();
    var ownerId = Guid.NewGuid();
    var entity = MakeReviewEntity(movieId: movieId, userId: ownerId);

    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.GetReviewAsync(movieId, entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    SetupValidatorValid();
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    var dto = MakeDto();
    await _sut.Update(movieId, entity.Id, dto, ownerId, canModerate: false);

    Assert.Equal(dto.Body, entity.Body);
    Assert.Equal(dto.Score, entity.Score);
    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task UpdatePut_WhenOwner_ResyncsAuthorNameFromOwnersCurrentDisplayName()
  {
    var movieId = Guid.NewGuid();
    var ownerId = Guid.NewGuid();
    var entity = MakeReviewEntity(movieId: movieId, userId: ownerId);
    _userManager
      .Setup(m => m.FindByIdAsync(ownerId.ToString()))
      .ReturnsAsync(new ApplicationUser { Id = ownerId, DisplayName = "Updated Display Name" });

    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.GetReviewAsync(movieId, entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    SetupValidatorValid();
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    await _sut.Update(movieId, entity.Id, MakeDto(), ownerId, canModerate: false);

    Assert.Equal("Updated Display Name", entity.AuthorName);
  }

  [Fact]
  public async Task UpdatePut_WhenNotOwnerButCanModerate_Saves()
  {
    var movieId = Guid.NewGuid();
    var entity = MakeReviewEntity(movieId: movieId, userId: Guid.NewGuid());

    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.GetReviewAsync(movieId, entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    SetupValidatorValid();
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    await _sut.Update(movieId, entity.Id, MakeDto(), Guid.NewGuid(), canModerate: true);

    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  // Update (PATCH)

  [Fact]
  public async Task UpdatePatch_WhenMovieNotFound_ThrowsNotFoundException()
  {
    var movieId = Guid.NewGuid();
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

    var error = await Assert.ThrowsAsync<NotFoundException>(
      () => _sut.Update(movieId, Guid.NewGuid(), new JsonPatchDocument<ReviewForChangeDto>(), Guid.NewGuid(), canModerate: true));

    Assert.Contains($"Movie '{movieId}' not found", error.Message);
  }

  [Fact]
  public async Task UpdatePatch_WhenReviewNotFound_ReturnsFalse()
  {
    var movieId = Guid.NewGuid();
    var id = Guid.NewGuid();
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.GetReviewAsync(movieId, id, It.IsAny<CancellationToken>())).ReturnsAsync((Review?)null);

    var error = await Assert.ThrowsAsync<NotFoundException>(
      () => _sut.Update(movieId, id, new JsonPatchDocument<ReviewForChangeDto>(), Guid.NewGuid(), canModerate: true));

    Assert.Contains($"Review '{id}' not found", error.Message);
  }

  [Fact]
  public async Task UpdatePatch_WhenNotOwnerAndCannotModerate_ThrowsForbiddenException()
  {
    var movieId = Guid.NewGuid();
    var entity = MakeReviewEntity(movieId: movieId, userId: Guid.NewGuid());
    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.GetReviewAsync(movieId, entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

    await Assert.ThrowsAsync<ForbiddenException>(
      () => _sut.Update(movieId, entity.Id, new JsonPatchDocument<ReviewForChangeDto>(), Guid.NewGuid(), canModerate: false));
  }

  [Fact]
  public async Task UpdatePatch_WhenOwnerAndPatchIsValid_Saves()
  {
    var movieId = Guid.NewGuid();
    var ownerId = Guid.NewGuid();
    var entity = MakeReviewEntity(movieId: movieId, userId: ownerId);
    var updateDto = MakeDto();

    _movieRepo.Setup(r => r.ExistsAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    _repo.Setup(r => r.GetReviewAsync(movieId, entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
    _mapper.Setup(m => m.Map<ReviewForChangeDto>(entity)).Returns(updateDto);
    SetupValidatorValid();
    _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

    await _sut.Update(movieId, entity.Id, new JsonPatchDocument<ReviewForChangeDto>(), ownerId, canModerate: false);

    _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }
}
