using Microsoft.Extensions.Configuration;
using Server.Interfaces;
using System.Net;
using System.Net.Mail;
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
            var from = new MailAddress(emailConfiruration["EmailConfig:From"]);
            var to = new MailAddress(toEmail);

            var message = new MailMessage(from, to);
            message.Subject = subject;
            message.Body = content;
            message.IsBodyHtml = true;

            var email = new SmtpClient(emailConfiruration["EmailConfig:Provider"], int.Parse(emailConfiruration["EmailConfig:Port"]));
            email.Credentials = new NetworkCredential(emailConfiruration["EmailConfig:Username"], emailConfiruration["EmailConfig:Password"]);
            email.EnableSsl = true;

            await email.SendMailAsync(message);
        }
    }
}
