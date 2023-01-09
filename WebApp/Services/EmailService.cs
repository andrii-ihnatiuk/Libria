using Libria.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Libria.Services
{
	public class EmailService : IEmailService
	{
		private readonly EmailSettings _emailSettings;
		private readonly ILogger<EmailService> _logger;

		public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
		{
			_emailSettings = options.Value;
			_logger = logger;
		}

		public async Task<EmailStatus> SendEmailAsync(List<string> toEmail, string subject, List<string> messages)
		{
			try
			{
				using (var smtp = new SmtpClient())
				{
					await smtp.ConnectAsync(_emailSettings.Server, _emailSettings.Port, SecureSocketOptions.StartTls);
					await smtp.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password);

					for (int i = 0; i < toEmail.Count; i++)
					{
						var mail = new MimeMessage();
						mail.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
						mail.To.Add(new MailboxAddress("", toEmail[i]));
						mail.Subject = subject;
						var body = new BodyBuilder();

						string message = (toEmail.Count == messages.Count) ? messages[i] : messages.First();
						body.HtmlBody = message;
						mail.Body = body.ToMessageBody();

						await smtp.SendAsync(mail);
					}
					await smtp.DisconnectAsync(true);
				}
				return EmailStatus.SuccessResult;
			}
			catch (Exception ex)
			{
				_logger.LogCritical(ex.Message);
				return EmailStatus.ErrorResult;
			}
		}
	}

	public enum EmailStatus
	{
		SuccessResult,
		ErrorResult
	}
}
