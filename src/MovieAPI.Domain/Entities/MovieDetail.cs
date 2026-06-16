using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieAPI.Domain.Entities;

public class MovieDetail
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

  // Navigation
  public Guid MovieId { get; set; }
  [ForeignKey("MovieId")]
  public Movie Movie { get; set; }
}
