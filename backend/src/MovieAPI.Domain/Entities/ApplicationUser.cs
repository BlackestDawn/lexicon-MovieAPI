using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using MovieAPI.Domain.Interfaces;

namespace MovieAPI.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>, ITrackable
{
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  [MaxLength(100)]
  public string DisplayName { get; set; } = string.Empty;
}
