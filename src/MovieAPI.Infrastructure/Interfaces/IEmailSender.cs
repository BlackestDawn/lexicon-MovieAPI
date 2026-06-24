namespace MovieAPI.Infrastructure.Interfaces;

public interface IEmailSender
{
  Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken token = default);
}
