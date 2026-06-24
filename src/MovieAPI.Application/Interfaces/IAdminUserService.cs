using MovieAPI.Application.Models;
using MovieAPI.Infrastructure.Models;

namespace MovieAPI.Application.Interfaces;

public interface IAdminUserService
{
  Task<(IEnumerable<AdminUserDto>, PaginationMetadata)> GetMany(int? page, int? pageSize, CancellationToken token = default);
  Task<AdminUserDto> GetOne(Guid id, CancellationToken token = default);
  Task<AdminUserDto> Create(AdminUserForCreationDto newUser, CancellationToken token = default);
  Task<AdminUserDto> Update(Guid id, AdminUserForUpdateDto updatedUser, Guid currentAdminId, CancellationToken token = default);
  Task Remove(Guid id, Guid currentAdminId, CancellationToken token = default);
}
