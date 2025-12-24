namespace Application.Interfaces.Services;

/// <summary>
/// Email service interface for sending various types of emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send email confirmation link
    /// </summary>
    Task SendConfirmationLinkAsync(string email, string userName, string link);

    /// <summary>
    /// Send password reset link
    /// </summary>
    Task SendPasswordResetLinkAsync(string email, string userName, string link);

    /// <summary>
    /// Send password reset code
    /// </summary>
    Task SendPasswordResetCodeAsync(string email, string userName, string code);

    /// <summary>
    /// Send generic email
    /// </summary>
    Task SendEmailAsync(string email, string subject, string htmlMessage);
}

