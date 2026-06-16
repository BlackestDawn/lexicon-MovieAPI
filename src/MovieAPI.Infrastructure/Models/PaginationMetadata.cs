namespace MovieAPI.Infrastructure.Models;

public class PaginationMetadata(int totalItemCount, int pageSize, int currentPage)
{
  public int TotalItemCount { get; init; } = totalItemCount;
  public int TotalPageCount = (int)Math.Ceiling(totalItemCount / (double)pageSize);
  public int PageSize { get; init; } = pageSize;
  public int CurrentPage { get; init; } = currentPage;
}
