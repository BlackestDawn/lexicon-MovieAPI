using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Models;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Application.Interfaces;

public interface IReviewService
{
  Task<(IEnumerable<ReviewDto>, PaginationMetadata?)> GetMany(Guid movieId, ReviewSearchParams searchParams, int? page, int? pageSize, CancellationToken token = default);
  Task<ReviewDto> GetOne(Guid movieId, Guid id, CancellationToken token = default);
  Task<ReviewDto> Create(Guid movieId, ReviewForChangeDto newReview, Guid currentUserId, CancellationToken token = default);
  Task Update(Guid movieId, Guid id, ReviewForChangeDto updatedReview, Guid currentUserId, bool canModerate, CancellationToken token = default);
  Task Update(Guid movieId, Guid id, JsonPatchDocument<ReviewForChangeDto> patchDocument, Guid currentUserId, bool canModerate, CancellationToken token = default);
  Task Remove(Guid movieId, Guid id, Guid currentUserId, bool canModerate, CancellationToken token = default);
}
