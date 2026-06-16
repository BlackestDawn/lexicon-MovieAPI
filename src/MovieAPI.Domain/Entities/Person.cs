using System.ComponentModel.DataAnnotations;

namespace MovieAPI.Domain.Entities;

public class Person
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

  // Navigation
  public ICollection<CastCrew> CastCrews { get; set; } = [];
}
