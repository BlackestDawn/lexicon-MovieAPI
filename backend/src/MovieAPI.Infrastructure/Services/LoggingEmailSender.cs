using Microsoft.Extensions.Logging;
using MovieAPI.Infrastructure.Interfaces;

namespace MovieAPI.Infrastructure.Services;

// Placeholder until a real provider (SMTP, SendGrid, etc.) is wired up: this just
// logs the reset token instead of emailing it. Whoever can read server logs has the
// same level of access a real implementation would give to whoever owns the mailbox,
// so this doesn't weaken the token's security - it just lacks real delivery.
public class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
  public Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken token = default)
  {
    logger.LogInformation(
      "Password reset requested for {Email}. Reset token (send this to POST /api/auth/reset-password): {ResetToken}",
      toEmail, resetToken);

    return Task.CompletedTask;
  }
}
