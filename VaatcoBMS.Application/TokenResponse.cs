namespace VaatcoBMS.Application;

public class TokenResponse
{
	public string AccessToken { get; set; }
	public string RefreshToken { get; set; }
	public DateTimeOffset ExpiresAtUtc { get; set; }
}
