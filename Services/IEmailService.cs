namespace IMS_API_.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
}
