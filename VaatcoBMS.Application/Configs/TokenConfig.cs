namespace VaatcoBMS.Application.Configs;
public class TokenConfig
{
	public string Audience { get; set; }
	public string Issuer { get; set; }
	public string SecretKey { get; set; }
	public int ExpiryInHours { get; set; }
}
