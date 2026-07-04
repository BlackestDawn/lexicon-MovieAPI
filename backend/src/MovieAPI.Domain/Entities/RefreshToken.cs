using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MovieAPI.Domain.Interfaces;

namespace MovieAPI.Domain.Entities;

public class RefreshToken : ITrackable, IEntity
{
  [Key]
  public Guid Id { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }

  public Guid UserId { get; set; }
  [ForeignKey("UserId")]
  public ApplicationUser User { get; set; } = null!;

  // Only the hash is stored - the raw token is handed to the client once and never
  // persisted, the same way passwords are never stored in plaintext.
  [MaxLength(128)]
  public string TokenHash { get; set; } = string.Empty;

  public DateTime ExpiresAtUtc { get; set; }
  public DateTime? RevokedAtUtc { get; set; }

  [NotMapped]
  public bool IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;
}
