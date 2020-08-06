using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Glaz.Server.Data.AppSettings;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace Glaz.Server.Data
{
    public sealed class EmailSender : IEmailSender
    {
        private readonly EmailSenderOptions _settings;
        public EmailSender(IOptions<EmailSenderOptions> options)
        {
            _settings = options.Value;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient
            {
                Host = _settings.Host,
                Port = _settings.Port,
                Credentials = new NetworkCredential(_settings.Credentials.Username, _settings.Credentials.Password),
                EnableSsl = _settings.EnableSsl
            };

            var message = new MailMessage(
                from: new MailAddress(_settings.From),
                to: new MailAddress(email))
            {
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
        }
    }
}
