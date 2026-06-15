using MovieAPI.Domain.Models;

namespace MovieAPI.Domain.Entities;

// Movie <-> Person juction table
public class CastCrew
{
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public PersonRole Role { get; set; }

  public Guid MovieId { get; set; }
  public Movie Movie { get; set; }

  public Guid PersonId { get; set; }
  public Person Person { get; set; }
}
