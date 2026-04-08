using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VaatcoBMS.Application.Settings;
using VaatcoBMS.Domain.Entities;

namespace VaatcoBMS.Infrastructure.Utility;

public class TokenBuilder(IOptions<JwtSettings> jwtSettings) : ITokenBuilder
{
	private readonly JwtSettings _jwtSettings = jwtSettings.Value;

	public string BuildToken(string email, int userId, string role)
	{
		var claims = new List<Claim>
		{
			new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
			new Claim(ClaimTypes.Email, email),
			new Claim(ClaimTypes.Role, role)
		};

		return GenerateJwtWithClaims(claims, _jwtSettings.DefaultExpiration);
	}

	public string BuildEmailToken(User user, string purpose)
	{
		var claims = new List<Claim>
		{
			new Claim(JwtRegisteredClaimNames.Sub, purpose),
			new Claim(JwtRegisteredClaimNames.Email, user.Email)
		};

		return GenerateJwtWithClaims(claims, _jwtSettings.EmailValidationTokenExpiration);
	}

	private string GenerateJwtWithClaims(IEnumerable<Claim> claims, int expirationDays)
	{
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret ?? string.Empty));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: _jwtSettings.Issuer,
			audience: _jwtSettings.Audience,
			claims: claims,
			expires: DateTime.UtcNow.AddDays(expirationDays),
			signingCredentials: creds
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	public bool IsJwtValid(string token)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret ?? string.Empty);
		try
		{
			tokenHandler.ValidateToken(token, new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidIssuer = _jwtSettings.Issuer,
				ValidAudience = _jwtSettings.Audience,
				ClockSkew = TimeSpan.Zero
			}, out _);
			return true;
		}
		catch
		{
			return false;
		}
	}

	public IEnumerable<Claim> GetClaims(string token)
	{
		var handler = new JwtSecurityTokenHandler();
		if (handler.ReadToken(token) is not JwtSecurityToken securityToken)
		{
			throw new Exception("Invalid Security token");
		}
		return securityToken.Claims;
	}
}

