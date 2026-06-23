using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Models;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Application.Interfaces;

public interface IReviewService
{
  Task<(IEnumerable<ReviewDto>, PaginationMetadata?)> GetMany(Guid movieId, ReviewSearchParams searchParams, int? page, int? pageSize, CancellationToken token = default);
  Task<ReviewDto> GetOne(Guid movieId, Guid id, CancellationToken token = default);
  Task<ReviewDto> Create(Guid movieId, ReviewForChangeDto newReview, CancellationToken token = default);
  Task Update(Guid movieId, Guid id, ReviewForChangeDto updatedReview, CancellationToken token = default);
  Task Update(Guid movieId, Guid id, JsonPatchDocument<ReviewForChangeDto> patchDocument, CancellationToken token = default);
  Task Remove(Guid movieId, Guid id, CancellationToken token = default);
}
