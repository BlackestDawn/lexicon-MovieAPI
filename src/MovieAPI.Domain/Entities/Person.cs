using System.ComponentModel.DataAnnotations;
using MovieAPI.Domain.Interfaces;

namespace MovieAPI.Domain.Entities;

public class Person : ITrackable, IEntity
{
  [Key]
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  [MaxLength(50)]
  public string FirstName { get; set; } = string.Empty;
  [MaxLength(50)]
  public string LastName { get; set; } = string.Empty;
  public DateOnly DateOfBirth { get; set; }

  public ICollection<CastCrew> CastCrews { get; set; } = [];
}
