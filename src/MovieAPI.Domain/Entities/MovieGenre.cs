using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieAPI.Domain.Entities;

// Movie <-> Genre junction table
public class MovieGenre
{
  [Key]
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }

  public Guid MovieId { get; set; }
  [ForeignKey("MovieId")]
  public Movie Movie { get; set; }

  public Guid GenreId { get; set; }
  [ForeignKey("GenreId")]
  public Genre Genre { get; set; }
}
