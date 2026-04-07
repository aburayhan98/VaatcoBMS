using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Application.Settings;

namespace VaatcoBMS.Infrastructure.Services;

public class EmailService(IOptions<SmtpSettings> settings, ILogger<EmailService> logger) : IEmailService
{
	private readonly SmtpSettings _settings = settings.Value;
	private readonly ILogger<EmailService> _logger = logger;

	public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
	{
		// Validate inputs
		ValidateEmailInputs(toEmail, subject, htmlMessage);

		var email = CreateEmailMessage(toEmail, subject, htmlMessage);

		try
		{
			using var smtp = new SmtpClient();

			await ConnectAndAuthenticateAsync(smtp);
			await SendEmailAsync(smtp, email, toEmail, subject);

			_logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
		}
		catch (Exception ex)
		{
			HandleEmailException(ex, toEmail, subject);
			throw;
		}
		finally
		{
			email.Dispose();
		}
	}

	private void ValidateEmailInputs(string toEmail, string subject, string htmlMessage)
	{
		if (string.IsNullOrWhiteSpace(toEmail))
		{
			_logger.LogWarning("Attempted to send email with null or empty recipient");
			throw new ArgumentException("Recipient email address cannot be empty.", nameof(toEmail));
		}

		if (string.IsNullOrWhiteSpace(subject))
		{
			_logger.LogWarning("Attempted to send email to {ToEmail} with empty subject", toEmail);
			throw new ArgumentException("Email subject cannot be empty.", nameof(subject));
		}

		if (string.IsNullOrWhiteSpace(htmlMessage))
		{
			_logger.LogWarning("Attempted to send email to {ToEmail} with empty body", toEmail);
			throw new ArgumentException("Email body cannot be empty.", nameof(htmlMessage));
		}
	}

	private MimeMessage CreateEmailMessage(string toEmail, string subject, string htmlMessage)
	{
		var email = new MimeMessage();
		email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
		email.To.Add(MailboxAddress.Parse(toEmail));
		email.Subject = subject;
		email.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

		return email;
	}

	private async Task ConnectAndAuthenticateAsync(SmtpClient smtp)
	{
		var secureOption = _settings.UseSsl
				? SecureSocketOptions.SslOnConnect
				: SecureSocketOptions.StartTls;

		_logger.LogInformation("Connecting to SMTP server {Server}:{Port}", _settings.Server, _settings.Port);

		await smtp.ConnectAsync(_settings.Server, _settings.Port, secureOption);
		await smtp.AuthenticateAsync(_settings.Username, _settings.Password);

		_logger.LogDebug("Authenticated as {Username}", _settings.Username);
	}

	private async Task SendEmailAsync(SmtpClient smtp, MimeMessage email, string toEmail, string subject)
	{
		_logger.LogInformation("Sending email to {ToEmail}: {Subject}", toEmail, subject);
		await smtp.SendAsync(email);
		await smtp.DisconnectAsync(true);
	}

	private void HandleEmailException(Exception ex, string toEmail, string subject)
	{
		switch (ex)
		{
			case SmtpCommandException smtpEx:
				_logger.LogError(smtpEx, "SMTP error sending to {ToEmail}. Status: {StatusCode}",
						toEmail, smtpEx.StatusCode);
				throw new ApplicationException($"SMTP error: {smtpEx.Message}", smtpEx);

			case SmtpProtocolException protocolEx:
				_logger.LogError(protocolEx, "SMTP protocol error sending to {ToEmail}", toEmail);
				throw new ApplicationException($"SMTP protocol error: {protocolEx.Message}", protocolEx);

			case AuthenticationException authEx:
				_logger.LogError(authEx, "Authentication failed for {Username}", _settings.Username);
				throw new ApplicationException("Email server authentication failed. Check your credentials.", authEx);

			case OperationCanceledException cancelEx:
				_logger.LogError(cancelEx, "Email sending cancelled for {ToEmail}", toEmail);
				throw new ApplicationException("Email sending was cancelled.", cancelEx);

			default:
				_logger.LogError(ex, "Unexpected error sending email to {ToEmail}: {Subject}", toEmail, subject);
				throw new ApplicationException($"Failed to send email to {toEmail}.", ex);
		}
	}
}