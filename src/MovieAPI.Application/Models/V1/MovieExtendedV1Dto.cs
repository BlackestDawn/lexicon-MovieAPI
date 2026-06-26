namespace MovieAPI.Application.Models.V1;

public class MovieExtendedV1Dto
{
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public string Title { get; set; } = string.Empty;
  public DateOnly ReleaseDate { get; set; }
  public string PlotSummery { get; set; } = string.Empty;
  public int RuntimeMinutes { get; set; }
  public ICollection<CastCrewV1Dto>? CastCrews { get; set; }
  public ICollection<GenreDto> Genres { get; set; } = [];
  public ICollection<ReviewDto> Reviews { get; set; } = [];
  public MovieDetailDto? Details { get; set; }
  public decimal AverageRating { get; set; }
}
