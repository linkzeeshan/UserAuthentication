using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System.Text;
using System.Threading.Tasks;
using UserManagementApp.Services.Models;

namespace UserManagementApp.Services.Services
{
    public class UserEmailService : IUserEmailService
    {
        private readonly EmailConfiguration _emailConfiguration;

        public UserEmailService(IOptions<EmailConfiguration> emailConfiguration)
        {
            _emailConfiguration = emailConfiguration.Value;
        }
        public async Task SendEmailAsyc(Message message)
        {
            var createEmail = CreateEmailMessage(message);
           await SendAsync(createEmail);
        }

        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Cdoxs email", _emailConfiguration.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(TextFormat.Text) { Text = message.Content };

            return emailMessage;
        }
        private async Task SendAsync(MimeMessage mailMessage)
        {
            using (SmtpClient mailClient = new SmtpClient())
            {

                mailClient.Connect(_emailConfiguration.SmtpServer, Int32.Parse(_emailConfiguration.Port), MailKit.Security.SecureSocketOptions.StartTls);
                mailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                mailClient.Authenticate(_emailConfiguration.UserName, _emailConfiguration.Password);
          await mailClient.SendAsync(mailMessage);
                mailClient.Disconnect(true);
            }
            
        }
    }
}
