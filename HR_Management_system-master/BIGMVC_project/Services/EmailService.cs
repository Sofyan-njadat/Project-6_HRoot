using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BIGMVC_project.Services
{
	public class EmailService
	{
		private readonly string _smtpServer = "smtp.gmail.com"; // SMTP Server
		private readonly int _smtpPort = 587; // SMTP Port
		private readonly string _smtpUsername = "sondos.3th@gmail.com";
		private readonly string _smtpPassword = "xlyl mhpy nzxt aved";
		public async Task SendEmailAsync(string toEmail, string subject, string body)
		{
			using (var client = new SmtpClient(_smtpServer, _smtpPort))
			{
				client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
				client.EnableSsl = true;

				var mailMessage = new MailMessage
				{
					From = new MailAddress(_smtpUsername),
					Subject = subject,
					Body = body,
					IsBodyHtml = true
				};
				mailMessage.To.Add(toEmail);

				await client.SendMailAsync(mailMessage);
			}
		}
	}
}