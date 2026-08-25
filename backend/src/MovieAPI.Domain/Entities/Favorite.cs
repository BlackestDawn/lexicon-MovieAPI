using System.ComponentModel.DataAnnotations.Schema;
using MovieAPI.Domain.Interfaces;

namespace MovieAPI.Domain.Entities;

// User <-> Movie favorites junction table
public class Favorite : ITrackable
{
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }

  public Guid UserId { get; set; }
  [ForeignKey("UserId")]
  public ApplicationUser User { get; set; } = null!;

  public Guid MovieId { get; set; }
  [ForeignKey("MovieId")]
  public Movie Movie { get; set; } = null!;
}
