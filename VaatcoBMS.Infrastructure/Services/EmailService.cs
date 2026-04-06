using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Application.Settings;

namespace VaatcoBMS.Infrastructure.Services;

public class EmailService : IEmailService
{
	private readonly SmtpSettings _settings;

	public EmailService(IOptions<SmtpSettings> settings)
	{
		_settings = settings.Value;
	}

	public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
	{
		var email = new MimeMessage();
		email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
		email.To.Add(MailboxAddress.Parse(toEmail));
		email.Subject = subject;
		email.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

		using var smtp = new SmtpClient();

		// Connect. Use SecureSocketOptions.StartTls for ports like 587, and SslOnConnect for 465.
		var secureSocketOption = _settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

		await smtp.ConnectAsync(_settings.Server, _settings.Port, secureSocketOption);
		await smtp.AuthenticateAsync(_settings.Username, _settings.Password);

		await smtp.SendAsync(email);
		await smtp.DisconnectAsync(true);
	}
}

//var secureSocketOption = _settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;