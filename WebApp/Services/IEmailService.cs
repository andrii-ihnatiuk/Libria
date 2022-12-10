namespace Libria.Services
{
	public interface IEmailService
	{
		Task<EmailStatus> SendEmailAsync(string toEmail, string subject, string message);
	}
}
