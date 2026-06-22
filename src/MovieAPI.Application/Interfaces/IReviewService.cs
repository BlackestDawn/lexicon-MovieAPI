using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Models;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Application.Interfaces;

public interface IReviewService
{
  Task<(IEnumerable<ReviewDto>, PaginationMetadata?)> GetMany(Guid movieId, ReviewSearchParams searchParams, int? page, int? pageSize, CancellationToken token = default);
  Task<ReviewDto?> GetOne(Guid movieId, Guid id, CancellationToken token = default);
  Task<ReviewCreationResult> Create(Guid movieId, ReviewForCreationDto newReview, CancellationToken token = default);
  Task<(bool, string?)> Update(Guid id, ReviewForUpdateDto updatedReview, CancellationToken token = default);
  Task<(bool, string?)> Update(Guid id, JsonPatchDocument<ReviewForUpdateDto> patchDocument, CancellationToken token = default);
  Task Remove(Guid id, CancellationToken token = default);
}
