using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Server.Interfaces;
using System.Threading.Tasks;

namespace Server.Services
{
    public class MailService : IMailService
    {
        private readonly IConfiguration emailConfiruration;

        public MailService(IConfiguration configuration) => emailConfiruration = configuration;

        /// <summary>
        /// Method for sending messages asynchronously
        /// </summary>
        /// <param name="toEmail"></param>
        /// <param name="subject"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task SendEmailAsync(string toEmail, string subject, string content)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("GGKTTD", emailConfiruration["EmailConfig:From"]));
            emailMessage.To.Add(MailboxAddress.Parse(toEmail));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = content,
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(emailConfiruration["EmailConfig:Provider"], int.Parse(emailConfiruration["EmailConfig:Port"]), false);
                await client.AuthenticateAsync(emailConfiruration["EmailConfig:Username"], emailConfiruration["EmailConfig:Password"]);
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
        }
    }
}
