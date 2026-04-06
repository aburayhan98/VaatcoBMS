namespace VaatcoBMS.Application
{
	public class AuthToken
	{
		public string Value { get; set; }

		public string Type { get; set; }

		public AuthToken()
		{
			Value = "";
			Type = "";
		}

		public AuthToken(string value)
		{
			Value = value;
			Type = "Bearer";
		}

		public AuthToken(string value, string type)
		{
			Value = value;
			Type = type;
		}
	}
}
