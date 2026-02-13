using FluentAssertions;
using Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Resend;

namespace Infrastructure.Tests.Identity;

public class EmailSenderServiceTests
{
    [Fact]
    public async Task SendEmailAsync_throws_when_api_token_missing()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var resend = new Mock<IResend>();
        var service = new EmailSenderService(resend.Object, config, NullLogger<EmailSenderService>.Instance);

        var act = async () => await service.SendEmailAsync("user@test.local", "subject", "<p>hi</p>");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SendConfirmationLinkAsync_sends_email()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Resend:ApiToken"] = "token",
                ["Resend:FromEmail"] = "from@test.local",
                ["ClientApp:ClientUrl"] = "https://client.test"
            })
            .Build();

        var resend = new Mock<IResend>();
        resend.Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync(new ResendResponse<Guid>(Guid.NewGuid(), new ResendRateLimit()));

        var service = new EmailSenderService(resend.Object, config, NullLogger<EmailSenderService>.Instance);

        await service.SendConfirmationLinkAsync("user@test.local", "User", "https://link");

        resend.Verify(r => r.EmailSendAsync(It.IsAny<EmailMessage>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_throws_when_resend_exception()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Resend:ApiToken"] = "token"
            })
            .Build();

        var resend = new Mock<IResend>();
        resend.Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>()))
            .ThrowsAsync(new ResendException(System.Net.HttpStatusCode.Unauthorized, ErrorType.InvalidApiKey, "bad", new ResendRateLimit()));

        var service = new EmailSenderService(resend.Object, config, NullLogger<EmailSenderService>.Instance);

        var act = async () => await service.SendEmailAsync("user@test.local", "subject", "<p>hi</p>");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SendEmailAsync_rethrows_unhandled_exception()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Resend:ApiToken"] = "token"
            })
            .Build();

        var resend = new Mock<IResend>();
        resend.Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var service = new EmailSenderService(resend.Object, config, NullLogger<EmailSenderService>.Instance);

        var act = async () => await service.SendEmailAsync("user@test.local", "subject", "<p>hi</p>");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("boom");
    }

    [Fact]
    public async Task SendPasswordResetLinkAsync_sends_email()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Resend:ApiToken"] = "token",
                ["Resend:FromEmail"] = "from@test.local",
                ["ClientApp:ClientUrl"] = "https://client.test"
            })
            .Build();

        EmailMessage? captured = null;
        var resend = new Mock<IResend>();
        resend.Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>()))
            .Callback<EmailMessage, CancellationToken>((m, _) => captured = m)
            .ReturnsAsync(new ResendResponse<Guid>(Guid.NewGuid(), new ResendRateLimit()));

        var service = new EmailSenderService(resend.Object, config, NullLogger<EmailSenderService>.Instance);

        await service.SendPasswordResetLinkAsync("user@test.local", "User", "https://reset");

        captured.Should().NotBeNull();
        captured!.To.Should().ContainSingle().Which.Email.Should().Be("user@test.local");
        captured.Subject.Should().Contain("Reset");
    }

    [Fact]
    public async Task SendPasswordResetCodeAsync_uses_client_url()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Resend:ApiToken"] = "token",
                ["Resend:FromEmail"] = "from@test.local",
                ["ClientApp:ClientUrl"] = "https://client.test"
            })
            .Build();

        EmailMessage? captured = null;
        var resend = new Mock<IResend>();
        resend.Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>()))
            .Callback<EmailMessage, CancellationToken>((m, _) => captured = m)
            .ReturnsAsync(new ResendResponse<Guid>(Guid.NewGuid(), new ResendRateLimit()));

        var service = new EmailSenderService(resend.Object, config, NullLogger<EmailSenderService>.Instance);

        await service.SendPasswordResetCodeAsync("user@test.local", "User", "code");

        captured.Should().NotBeNull();
        captured!.HtmlBody.Should().Contain("https://client.test/reset-password");
    }
}
