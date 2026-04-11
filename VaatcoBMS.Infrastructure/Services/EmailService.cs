using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading;
using System.Threading.Tasks;
using VaatcoBMS.Application.Interfaces;
using VaatcoBMS.Application.Settings;

namespace VaatcoBMS.Infrastructure.Services;

public class EmailService : IEmailService
{
	private readonly SmtpSettings _smtp;
	private readonly ILogger<EmailService> _logger;

	public EmailService(IOptions<SmtpSettings> smtpOptions, ILogger<EmailService> logger)
	{
		_smtp = smtpOptions.Value;
		_logger = logger;
	}

	// Implementation that matches the IEmailService interface
	public Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
	{
		// Delegate to the overload that supports cancellation (use default CancellationToken)
		return SendEmailAsync(toEmail, subject, htmlMessage, CancellationToken.None);
	}

	// Existing implementation with optional CancellationToken
	public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
	{
		var message = new MimeMessage();
		message.From.Add(MailboxAddress.Parse(_smtp.SenderEmail));
		message.To.Add(MailboxAddress.Parse(to));
		message.Subject = subject;

		var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
		message.Body = bodyBuilder.ToMessageBody();

		using var client = new SmtpClient();

		try
		{
			// Choose correct SecureSocketOptions for common SMTP ports
			SecureSocketOptions socketOptions;
			if (_smtp.Port == 465)
				socketOptions = SecureSocketOptions.SslOnConnect;       // implicit SSL
			else if (_smtp.Port == 587)
				socketOptions = SecureSocketOptions.StartTls;          // STARTTLS on 587
			else
				socketOptions = _smtp.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;

			_logger.LogDebug("Connecting to SMTP {Server}:{Port} using {SocketOptions}", _smtp.Server, _smtp.Port, socketOptions);

			// Connect
			await client.ConnectAsync(_smtp.Server, _smtp.Port, socketOptions, cancellationToken);

			// Authenticate if credentials provided
			if (!string.IsNullOrEmpty(_smtp.Username))
			{
				await client.AuthenticateAsync(_smtp.Username, _smtp.Password, cancellationToken);
			}

			await client.SendAsync(message, cancellationToken);

			_logger.LogInformation("Email sent to {To}", to);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send email to {To}", to);
			throw; // Let caller handle/log as appropriate
		}
		finally
		{
			try { await client.DisconnectAsync(true, cancellationToken); } catch { /* ignore */ }
		}
	}
}