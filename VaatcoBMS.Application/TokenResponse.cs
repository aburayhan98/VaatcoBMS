namespace VaatcoBMS.Application;

public class TokenResponse
{
	public required string AccessToken { get; set; }
	public required string RefreshToken { get; set; }
	public DateTimeOffset ExpiresAtUtc { get; set; }
}
