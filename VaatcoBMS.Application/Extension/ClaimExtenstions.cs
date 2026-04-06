using System.Security.Claims;

namespace VaatcoBMS.Application.Extension;

public static class ClaimExtensions
{
	public static int GetUserId(this IEnumerable<Claim> claims)
	{
		var value = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
		int.TryParse(value, out var userId);
		return userId;
	}
}
