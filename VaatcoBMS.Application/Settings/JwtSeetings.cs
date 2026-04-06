namespace VaatcoBMS.Application.Settings;

public class JwtSettings
{
	public string Secret { get; set; }
	public string Issuer { get; set; }
	public string Audience { get; set; }
	public int EmailValidationTokenExpiration { get; set; }
	public int DefaultExpiration { get; set; }
	public int AdminExpiration { get; set; }
}
