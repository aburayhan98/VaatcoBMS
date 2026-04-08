using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VaatcoBMS.Application;
using VaatcoBMS.Application.Settings;
using VaatcoBMS.Domain.Entities;

namespace VaatcoBMS.Infrastructure.Utility;
/// <summary>
/// Token builder for encoding and decoding JWT tokens
/// </summary>
public class TokenBuilder(IOptions<JwtSettings>
		jwtSettings) : ITokenBuilder
{
	private readonly JwtSettings _jwtSettings = jwtSettings.Value;

	/// <summary>
	/// Generate token to validate user email
	/// </summary>
	/// param name="email"></param>
	/// returns></returns>

	public AuthToken BuildEmailToken(User user, string sub)
	{
		var expirationTime = DateTime.UtcNow.AddDays(_jwtSettings.EmailValidationTokenExpiration);

		var claims = new List<Claim>
		{
		new(JwtRegisteredClaimNames.Sub, sub),
								new(ClaimTypes.Role, user.Role.ToString()), // FIXED: .ToString() to get string value
								new(JwtRegisteredClaimNames.Email, user.Email),
								new(JwtRegisteredClaimNames.Aud, _jwtSettings.Audience),
								new(JwtRegisteredClaimNames.Iss, _jwtSettings.Issuer),
								new(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString()),
								new(JwtRegisteredClaimNames.Exp, expirationTime.ToString()),
								new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

		var jwtTokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(claims),
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
		};
		var token = jwtTokenHandler.CreateToken(tokenDescriptor);
		var jwtToken = jwtTokenHandler.WriteToken(token);
		return new AuthToken(jwtToken);
	}
	/// <summary>
	/// Build JWT
	/// </summary>
	/// <param name="user"></param>
	/// <returns></returns>

	public AuthToken BuildToken(User user)
	{
		var expirationTime = DateTime.UtcNow.AddDays(_jwtSettings.DefaultExpiration);
		var claims = new List<Claim>
		{
								new(ClaimTypes.NameIdentifier, user.Id.ToString()),
								new(ClaimTypes.Role, user.Role.ToString()), // FIXED: .ToString() to get string value
								new(JwtRegisteredClaimNames.Email, user.Email),
								new(JwtRegisteredClaimNames.Aud, _jwtSettings.Audience),
								new(JwtRegisteredClaimNames.Iss, _jwtSettings.Issuer),
								new(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString()),
								new(JwtRegisteredClaimNames.Exp, expirationTime.ToString()),
								new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};
		var jwtTokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(claims),
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
		};
		var token = jwtTokenHandler.CreateToken(tokenDescriptor);
		var jwtToken = jwtTokenHandler.WriteToken(token);
		return new AuthToken(jwtToken);
	}

	/// <summary>
	/// Check whether the token is valid or not
	/// </summary>
	/// <param name="token"></param>
	/// <returns></returns>

	public bool IsJwtValid(string token)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
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
			}, out SecurityToken validatedToken);
			var jwtToken = (JwtSecurityToken)validatedToken;

			//return true if the token is thr validation was successfull
			return true;
		}
		catch
		{
			//return false if the validation failed
			return false;
		}
	}

	/// <summary>
	/// Check token expiration
	/// </summary>
	/// <param name="token"></param>
	/// <returns></returns>
	public bool IsExpired(string token)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		try
		{
			tokenHandler.ValidateToken(token, new TokenValidationParameters
			{
				ValidateLifetime = true,
				ValidIssuer = _jwtSettings.Issuer,
				ValidAudience = _jwtSettings.Audience,
				// set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
				ClockSkew = TimeSpan.Zero
			}, out SecurityToken validatedToken);

			var jwtToken = (JwtSecurityToken)validatedToken;

			// return true if the validation was succesfull
			return true;
		}
		catch
		{
			return false;
		}
	}

	public dynamic Verify(string token)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
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
			}, out SecurityToken validatedToken);
			var jwtToken = (JwtSecurityToken)validatedToken;

			//return true if the validation was successfull
			return GetClaims(token);
		}
		catch (Exception ex)
		{
			return ex;
		}
	}

	/// <summary>
	/// Verify token for password reset is valid
	/// </summary>
	/// <param name="token"></param>
	/// <returns></returns>

	public bool VerifyPasswordResetToken(User user, string token)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
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
			}, out SecurityToken validatedToken);
			var jwtToken = (JwtSecurityToken)validatedToken;
			//return true if the validation was successfull
			return true;
		}
		catch
		{
			return false;
		}
	}
	/// <summary>
	/// Get claims from JWT
	/// </summary>
	/// <param name="token"></param>
	/// <returns></returns>
	
	public IEnumerable<Claim> GetClaims(string token)
	{
		var handler = new JwtSecurityTokenHandler();

		if (handler.ReadToken(token) is not JwtSecurityToken securityToken)
		{
			throw new Exception("Invalid Security token");
		}
		return securityToken.Claims;
	}

	public IEnumerable<Claim> DecodeToken(string token)
	{
		return GetClaims(token);
	}

	public string BuildToken(string email, int userId, string role)
	{
		var claims = new List<Claim>
				{
						new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
						new Claim(ClaimTypes.Email, email),
						new Claim(ClaimTypes.Role, role)
				};

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret ?? string.Empty));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
				issuer: _jwtSettings.Issuer,
				audience: _jwtSettings.Audience,
				claims: claims,
				expires: DateTime.UtcNow.AddDays(_jwtSettings.DefaultExpiration),
				signingCredentials: creds
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}


	// 2. Email Verification Token (Secure Random String)
	// This is much safer and shorter for appending to Email URLs than a massive JWT.
	public string BuildEmailToken()
	{
		// Generates a URL-safe secure random token (e.g., for "Click here to verify email")
		var randomBytes = new byte[32];
		using (var rng = RandomNumberGenerator.Create())
		{
			rng.GetBytes(randomBytes);
		}

		// Convert to Base64 and make it URL-safe
		return Convert.ToBase64String(randomBytes)
			.Replace("+", "-")
			.Replace("/", "_")
			.Replace("=", "");
	}
}

