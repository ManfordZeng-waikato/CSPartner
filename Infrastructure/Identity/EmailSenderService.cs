using Microsoft.AspNetCore.Identity;
using Infrastructure.Persistence.Identity;
using Microsoft.Extensions.Configuration;
using Resend;

namespace Infrastructure.Identity;

public class EmailSenderService : IEmailSender<ApplicationUser>
{
    private readonly IResend _resend;
    private readonly string _fromEmail;
    
    public EmailSenderService(IResend resend, IConfiguration configuration)
    {
        _resend = resend;
        _fromEmail = configuration["Resend:FromEmail"] ?? "onboarding@resend.dev";
    }

    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string link)
    {
        var subject = "Confirm your email";
        var htmlMessage = $@"
            <h2>Please confirm your email address</h2>
            <p>Hi {user.UserName},  Click the link below to confirm your email:</p>
            <p><a href=""{link}"">Click here to confirm your email</a></p>
        ";
        await SendEmailAsync(email, subject, htmlMessage);
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string link)
    {
        // TODO: Implement email sending using Resend API
        // For now, this is a placeholder implementation
        await Task.CompletedTask;
    }

    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string code)
    {
        // TODO: Implement email sending using Resend API
        // For now, this is a placeholder implementation
        await Task.CompletedTask;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new EmailMessage
        {
            From = _fromEmail,
            To = email,
            Subject = subject,
            HtmlBody = htmlMessage
        };
        Console.WriteLine($"Sending email from {_fromEmail} to {email}");
        Console.WriteLine(message.HtmlBody);
        await _resend.EmailSendAsync(message);
    }
}