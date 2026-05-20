using System.Net;
using System.Net.Mail;
using IMS_API_.Models;
using Microsoft.Extensions.Options;

namespace IMS_API_.Services;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailSettings> options, ILogger<SmtpEmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            _logger.LogWarning("SMTP host not configured. Cannot send email.");
            throw new InvalidOperationException("SMTP host is not configured.");
        }

        var from = string.IsNullOrWhiteSpace(_settings.FromEmail) ? _settings.Username : _settings.FromEmail;
        using var msg = new MailMessage(from, to, subject, htmlBody) { IsBodyHtml = true };

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(_settings.Username))
        {
            client.Credentials = new NetworkCredential(_settings.Username, _settings.Password ?? string.Empty);
        }

        try
        {
            client.Send(msg);
            _logger.LogInformation("Email sent to {to}", to);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {to}", to);
            throw;
        }
    }
}
