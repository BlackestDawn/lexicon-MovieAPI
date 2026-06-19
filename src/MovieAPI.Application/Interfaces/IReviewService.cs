using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using MovieAPI.Application.Models;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Application.Interfaces;

public interface IReviewService
{
  Task<(IEnumerable<ReviewDto>, PaginationMetadata?)> GetMany(string? search, int? page, int? pageSize, CancellationToken token = default);
  Task<ReviewDto?> GetOne(Guid id, bool includePeople = false, CancellationToken token = default);
  Task<ReviewCreationResult> Create(ReviewForCreationDto newReview, CancellationToken token = default);
  Task<(bool, string?)> Update(Guid id, ReviewForUpdateDto updatedReview, CancellationToken token = default);
  Task<(bool, string?)> Update(Guid id, JsonPatchDocument<ReviewForUpdateDto> patchDocument, CancellationToken token = default);
  Task Remove(Guid id, CancellationToken token = default);
}
