namespace Libria.Services
{
	public interface IEmailService
	{
		Task<EmailStatus> SendEmailAsync(List<string> toEmail, string subject, List<string> messages);
	}
}
