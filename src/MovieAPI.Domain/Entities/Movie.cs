using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MovieAPI.Domain.Interfaces;

namespace MovieAPI.Domain.Entities;

public class Movie : ITrackable, IEntity
{
  [Key]
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  [MaxLength(100)]
  public string Title { get; set; } = string.Empty;
  public DateOnly ReleaseDate { get; set; }
  [MaxLength(200)]
  public string PlotSummery { get; set; } = string.Empty;
  public int RuntimeMinutes { get; set; }

  public ICollection<CastCrew> CastCrews { get; set; } = [];
  public ICollection<MovieGenre> MovieGenres { get; set; } = [];
  public ICollection<Review> Reviews { get; set; } = [];
  public MovieDetail Details { get; set; } = null!;

  [NotMapped]
  public decimal AverageRating => Reviews.Count > 0 ? Reviews.Sum(r => r.Score) / Reviews.Count : 0;
}
