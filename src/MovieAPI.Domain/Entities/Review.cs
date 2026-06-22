using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MovieAPI.Domain.Interfaces;

namespace MovieAPI.Domain.Entities;

public class Review : ITrackable, IEntity
{
  [Key]
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  [MaxLength(50)]
  public string AuthorName { get; set; } = string.Empty;
  [MaxLength(1000)]
  public string Body { get; set; } = string.Empty;
  public int Score { get; set; }

  public Guid MovieId { get; set; }
  [ForeignKey("MovieId")]
  public Movie Movie { get; set; } = null!;
}
