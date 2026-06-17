using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MovieAPI.Domain.Interfaces;

namespace MovieAPI.Domain.Entities;

public class MovieDetail : ITrackable
{
  [Key]
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  [MaxLength(10000)]
  public string Synopsis { get; set; } = string.Empty;
  [MaxLength(20)]
  public string Language { get; set; } = string.Empty;
  public int Budget { get; set; }

  public Guid MovieId { get; set; }
  [ForeignKey("MovieId")]
  public Movie Movie { get; set; } = null!;
}
