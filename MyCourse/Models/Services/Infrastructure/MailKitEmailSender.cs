using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MyCourse.Models.Options;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace MyCourse.Models.Services.Infrastructure
{
    public class MailKitEmailSender : IEmailClient
    {
        private readonly IOptionsMonitor<SmtpOptions> smtpOptionsMonitor;
        private readonly ILogger<MailKitEmailSender> logger;
        public MailKitEmailSender(IOptionsMonitor<SmtpOptions> smtpOptionsMonitor, ILogger<MailKitEmailSender> logger)
        {

            this.logger = logger;
            this.smtpOptionsMonitor = smtpOptionsMonitor;
        }
          public Task SendEmailAsync(string email, string subject, string htmlMessage)
          {
               return SendEmailAsync(email, string.Empty, subject, htmlMessage);
          }
        public async Task SendEmailAsync(string recipientEmail, string replyToEmail, string subject, string htmlMessage, CancellationToken token = default)
        {
            try
            {
                var options = this.smtpOptionsMonitor.CurrentValue;
                using var client = new SmtpClient();
                await client.ConnectAsync(options.Host, options.Port, options.Security);
                if (!string.IsNullOrEmpty(options.Username))
                {
                    await client.AuthenticateAsync(options.Username, options.Password);
                }
                var message = new MimeMessage();
                message.From.Add(MailboxAddress.Parse(options.Sender));
                message.To.Add(MailboxAddress.Parse(recipientEmail));
                if (replyToEmail is not(null or ""))
                {
                    message.ReplyTo.Add(MailboxAddress.Parse(replyToEmail));
                }
                message.Subject = subject;
                message.Body = new TextPart("html")
                {
                    Text = htmlMessage
                };
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception exc)
            {
                logger.LogError(exc, "Couldn't send email to {email} with message {message}", recipientEmail, htmlMessage);
            }
        }

     }
}