using System.ComponentModel.DataAnnotations;

namespace MovieAPI.Domain.Entities;

public class Genre
{
  [Key]
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  [MaxLength(50)]
  public string Name { get; set; } = string.Empty;
  [MaxLength(50)]
  public string Slug { get; set; } = string.Empty;

  // Navigation
  public ICollection<Movie> Movies {get; set;} = [];
}
