namespace MovieAPI.Application.Models;

public class MovieForCreationDto
{
  public string Title { get; set; } = string.Empty;
  public DateOnly ReleaseDate { get; set; }
  public string PlotSummery { get; set; } = string.Empty;
  public int RuntimeMinutes { get; set; }
  public ICollection<CastCrewForCreationDto> CastCrews { get; set; } = [];
  public ICollection<Guid> Genres { get; set; } = [];
  public string Synopsis { get; set; } = string.Empty;
  public string Language { get; set; } = string.Empty;
  public int Budget { get; set; }
}
