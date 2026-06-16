using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MovieAPI.Domain.Interfaces;
using MovieAPI.Domain.Models;

namespace MovieAPI.Domain.Entities;

// Movie <-> Person juction table
public class CastCrew : ITrackable
{
  [Key]
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public PersonRole Role { get; set; }

  public Guid MovieId { get; set; }
  [ForeignKey("MovieId")]
  public Movie Movie { get; set; }

  public Guid PersonId { get; set; }
  [ForeignKey("PersonId")]
  public Person Person { get; set; }
}
