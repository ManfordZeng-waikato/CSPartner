using Microsoft.AspNetCore.Identity;
using Infrastructure.Persistence.Identity;
using Resend;

namespace Infrastructure.Identity;

public class EmailSenderService : IEmailSender<ApplicationUser>
{
    private readonly IResend _resend;
    public EmailSenderService(IResend resend)
    {
        _resend = resend;
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
        var message = new EmailMessage{
            From = "noreply@highlighthub.local",
            To = email,
            Subject = subject,
            HtmlBody = htmlMessage
        };
        message.To.Add(email);
        Console.WriteLine(message.HtmlBody);
        // await _resend.EmailSendAsync(message);
        await Task.CompletedTask;
    }
}