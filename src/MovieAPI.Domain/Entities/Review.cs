using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieAPI.Domain.Entities;

public class Review
{
  [Key]
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  [MaxLength(50)]
  public string AuthorName { get; set; } = string.Empty;
  [MaxLength(400)]
  public string Body { get; set; } = string.Empty;
  public int Score { get; set; }

  // Navigation
  public Guid MovieId { get; set; }
  [ForeignKey("MovieId")]
  public Movie Movie { get; set; }
}
