using Application.Interfaces.Services;
using Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Resend;

namespace Infrastructure.Identity;

/// <summary>
/// Email service implementation that adapts IEmailSender to IEmailService
/// </summary>
public class EmailService : IEmailService
{
    private readonly IEmailSender<ApplicationUser> _emailSender;
    private readonly IResend _resend;
    private readonly string _fromEmail;

    public EmailService(
        IEmailSender<ApplicationUser> emailSender, 
        IResend resend,
        IConfiguration configuration)
    {
        _emailSender = emailSender;
        _resend = resend;
        _fromEmail = configuration["Resend:FromEmail"] ?? "onboarding@resend.dev";
    }

    public async Task SendConfirmationLinkAsync(string email, string userName, string link)
    {
        // Create a temporary user object for the adapter
        var user = new ApplicationUser { UserName = userName, Email = email };
        await _emailSender.SendConfirmationLinkAsync(user, email, link);
    }

    public async Task SendPasswordResetLinkAsync(string email, string userName, string link)
    {
        var user = new ApplicationUser { UserName = userName, Email = email };
        await _emailSender.SendPasswordResetLinkAsync(user, email, link);
    }

    public async Task SendPasswordResetCodeAsync(string email, string userName, string code)
    {
        var user = new ApplicationUser { UserName = userName, Email = email };
        await _emailSender.SendPasswordResetCodeAsync(user, email, code);
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // Directly implement SendEmailAsync using Resend
        var message = new EmailMessage
        {
            From = _fromEmail,
            To = email,
            Subject = subject,
            HtmlBody = htmlMessage
        };
        // TODO: Uncomment when Resend API is properly configured
        // await _resend.EmailSendAsync(message);
        await Task.CompletedTask;
    }
}

