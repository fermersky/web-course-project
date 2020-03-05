using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TodoApp.Business.Smtp.Abstract;
using TodoApp.Business.Smtp.Models;

namespace TodoApp.Business.Smtp.Implementations
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(EmailConfiguration emailConfig, ILogger<EmailSender> logger)
        {
            _emailConfig = emailConfig;
            _logger = logger;
        }

        public async Task SendEmailAsync(Message message)
        {
            var emailMessage = CreateEmailMessage(message);
            await SendAsync(emailMessage);
        }

        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(_emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message.Content };

            return emailMessage;
        }

        private async Task SendAsync(MimeMessage emailMessage)
        {
            using var client = new SmtpClient();

            try
            {
                client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, useSsl: true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);

                await client.SendAsync(emailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError("An exception was occured while sending email", ex.Message);
            }
            finally
            {
                client.Disconnect(quit: true);
                client.Dispose();
            }
        }
    }
}
