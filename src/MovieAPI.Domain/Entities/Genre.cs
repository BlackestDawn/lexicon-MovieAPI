using System.ComponentModel.DataAnnotations;
using MovieAPI.Domain.Interfaces;

namespace MovieAPI.Domain.Entities;

public class Genre : ITrackable, IEntity
{
  [Key]
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  [MaxLength(50)]
  public string Name { get; set; } = string.Empty;
  [MaxLength(50)]
  public string Slug { get; set; } = string.Empty;

  public ICollection<MovieGenre> MovieGenres { get; set; } = [];
}
