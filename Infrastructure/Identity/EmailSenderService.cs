using Microsoft.AspNetCore.Identity;
using Infrastructure.Persistence.Identity;
using Microsoft.Extensions.Configuration;
using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Resend;

namespace Infrastructure.Identity;

/// <summary>
/// Email service implementation that satisfies both:
/// 1. IEmailService (Application layer interface) - for business logic
/// 2. IEmailSender&lt;ApplicationUser&gt; (ASP.NET Core Identity interface) - for framework integration
/// </summary>
public class EmailSenderService : IEmailSender<ApplicationUser>, IEmailService
{
    private readonly IResend _resend;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSenderService> _logger;

    public EmailSenderService(IResend resend, IConfiguration configuration, ILogger<EmailSenderService> logger)
    {
        _resend = resend;
        _configuration = configuration;
        _logger = logger;
    }

    #region IEmailSender<ApplicationUser> implementation (for ASP.NET Core Identity)

    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string link)
    {
        await SendConfirmationLinkAsync(email, user.UserName ?? email, link);
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string link)
    {
        await SendPasswordResetLinkAsync(email, user.UserName ?? email, link);
    }

    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string code)
    {
        await SendPasswordResetCodeAsync(email, user.UserName ?? email, code);
    }

    #endregion

    #region IEmailService implementation (for Application layer)

    public async Task SendConfirmationLinkAsync(string email, string userName, string link)
    {
        var subject = "Confirm your email";
        var htmlMessage = BuildConfirmationEmailTemplate(userName, link);
        await SendEmailAsync(email, subject, htmlMessage);
    }

    public async Task SendPasswordResetLinkAsync(string email, string userName, string link)
    {
        var subject = "Reset your password";
        var htmlMessage = BuildPasswordResetLinkEmailTemplate(userName, link);
        await SendEmailAsync(email, subject, htmlMessage);
    }

    public async Task SendPasswordResetCodeAsync(string email, string userName, string code)
    {
        var subject = "Reset your password";
        var resetLink = $"{_configuration["ClientApp:ClientUrl"]}/reset-password?email={email}&code={code}";
        var htmlMessage = BuildPasswordResetCodeEmailTemplate(userName, resetLink);
        await SendEmailAsync(email, subject, htmlMessage);
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var fromEmail = _configuration["Resend:FromEmail"] ?? "onboarding@resend.dev";
        var apiToken = _configuration["Resend:ApiToken"];
        
        if (string.IsNullOrWhiteSpace(apiToken))
        {
            _logger.LogError("Resend API token is not configured. Cannot send email to {Email}", email);
            throw new InvalidOperationException("Resend API token is not configured. Please check your appsettings.json file.");
        }

        var message = new EmailMessage
        {
            From = fromEmail,
            To = email,
            Subject = subject,
            HtmlBody = htmlMessage
        };

        try
        {
            await _resend.EmailSendAsync(message);
        }
        catch (Resend.ResendException ex)
        {
            _logger.LogError(ex, "Resend API error when sending email to {Email}. Error: {ErrorMessage}. " +
                "Please verify that your Resend API token is valid and the FromEmail domain is verified in Resend.", 
                email, ex.Message);
            throw new InvalidOperationException(
                $"Failed to send email: {ex.Message}. Please verify your Resend API configuration.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} from {FromEmail}. Error: {ErrorMessage}", email, fromEmail, ex.Message);
            throw; // Re-throw to let caller handle the error
        }
    }

    #endregion

    #region Private helper methods for email templates

    private static string BuildConfirmationEmailTemplate(string userName, string link)
    {
        return $@"
            <h2>Please confirm your email address</h2>
            <p>Hi {userName},  Click the link below to confirm your email:</p>
            <p><a href=""{link}"">Click here to confirm your email</a></p>
            <p>If you did not request a email confirmation, please ignore this email.</p>
            <p>Thank you for using our application.</p>
            <p>Best regards,</p>
            <p>The CSPartner Team</p>
        ";
    }

    private static string BuildPasswordResetLinkEmailTemplate(string userName, string link)
    {
        return $@"
            <h2>Reset your password</h2>
            <p>Hi {userName},  Click the link below to reset your password:</p>
            <p><a href=""{link}"">Click here to reset your password</a></p>
            <p>If you did not request a password reset, please ignore this email.</p>
            <p>Thank you for using our application.</p>
            <p>Best regards,</p>
            <p>The CSPartner Team</p>
        ";
    }

    private static string BuildPasswordResetCodeEmailTemplate(string userName, string resetLink)
    {
        return $@"
            <h2>Reset your password</h2>
            <p>Hi {userName},  Click the link below to reset your password:</p>
            <p>
            <a href=""{resetLink}"">
            Click here to reset your password
            </a>
            </p>
            <p>If you did not request a password reset, please ignore this email.</p>
            <p>Thank you for using our application.</p>
            <p>Best regards,</p>
            <p>The CSPartner Team</p>
        ";
    }

    #endregion
}