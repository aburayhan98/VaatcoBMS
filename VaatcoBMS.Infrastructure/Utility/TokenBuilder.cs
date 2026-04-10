using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VaatcoBMS.Application;
using VaatcoBMS.Application.Settings;
using VaatcoBMS.Domain.Entities;

namespace VaatcoBMS.Infrastructure.Utility;

public class TokenBuilder(IOptions<JwtSettings> jwtSettings) : ITokenBuilder
{
	private const string RefreshTokenClaimType = "r:id";
	private readonly JwtSettings _jwtSettings = jwtSettings.Value;

	public TokenResponse BuildTokens(string email, int userId, string name, string role)
	{
		var handler = new JwtSecurityTokenHandler();
		var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret ?? string.Empty));
		var expires = DateTime.UtcNow.AddDays(_jwtSettings.DefaultExpiration);

		// 1. Create Access Token
		var claims = new List<Claim>
		{
			new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
			new Claim(ClaimTypes.Name, name),
			new Claim(ClaimTypes.Email, email),
			new Claim(ClaimTypes.Role, role)
		};

		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(claims),
			Expires = expires,
			Issuer = _jwtSettings.Issuer,
			Audience = _jwtSettings.Audience,
			SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
		};
		var accessToken = handler.CreateToken(tokenDescriptor);

		// 2. Create Refresh Token (typically lives longer, e.g., 7 days)
		var refreshClaims = new List<Claim>
		{
			new Claim(RefreshTokenClaimType, userId.ToString()),
			new Claim(ClaimTypes.Name, name),
			new Claim(ClaimTypes.Email, email),
			new Claim(ClaimTypes.Role, role)
		};
		
		var refreshTokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(refreshClaims),
			Expires = DateTime.UtcNow.AddDays(7), 
			Issuer = _jwtSettings.Issuer,
			Audience = _jwtSettings.Audience,
			SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
		};
		var refreshToken = handler.CreateToken(refreshTokenDescriptor);

		return new TokenResponse
		{
			AccessToken = handler.WriteToken(accessToken),
			ExpiresAtUtc = expires,
			RefreshToken = handler.WriteToken(refreshToken)
		};
	}

	public TokenResponse RefreshTokens(string refreshToken)
	{
		try
		{
			var handler = new JwtSecurityTokenHandler();
			var result = handler.ValidateToken(refreshToken, new TokenValidationParameters
			{
				ValidateAudience = true,
				ValidateIssuer = true,
				ValidateLifetime = true, // Ensure the refresh token isn't expired
				ValidateIssuerSigningKey = true,
				ValidAudience = _jwtSettings.Audience,
				ValidIssuer = _jwtSettings.Issuer,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret ?? string.Empty))
			}, out _);

			// Extract claims from the valid refresh token to generate a new pair
			var userIdVal = result.Claims.FirstOrDefault(c => c.Type == RefreshTokenClaimType)?.Value;
			var userName = result.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
			var userEmail = result.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
			var userRole = result.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

			if (userIdVal == null || userName == null || userEmail == null || userRole == null) 
                return null;

			// Generate a fresh set of tokens!
			return BuildTokens(userEmail, int.Parse(userIdVal), userName, userRole);
		}
		catch
		{
			return null; // Invalid or expired refresh token
		}
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

