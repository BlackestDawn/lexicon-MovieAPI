using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieAPI.Domain.Entities;

public class Movie
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

  // Navigation
  public ICollection<Person> People { get; set; } = [];
  public ICollection<Genre> Genres { get; set; } = [];
  public ICollection<Review> Reviews { get; set; } = [];

  // Computed
  [NotMapped]
  public decimal AverageRating => Reviews.Sum(r => r.Score) / Reviews.Count;
}
