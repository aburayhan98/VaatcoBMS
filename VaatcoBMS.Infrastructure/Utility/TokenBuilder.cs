
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VaatcoBMS.Application;
using VaatcoBMS.Application.Seetings;
using VaatcoBMS.Domain.Entities;

namespace VaatcoBMS.Infrastructure.Utility;
/// <summary>
/// Token builder for encoding and decoding JWT tokens
/// </summary>
public class TokenBuilder : ItokenBuilder
{
	private JwtSettings _jwtSeetings;

	public TokenBuilder(IOptions<JwtSettings>
		jwtSeetings)
	{
		_jwtSeetings = jwtSeetings.Value;
	}

	/// <summary>
	/// Generate token to validate user email
	/// </summary>
	/// param name="email"></param>
	/// returns></returns>

	public AuthToken BuildEmailToken(User user, string sub)
	{
		var expirationTime = DateTime.UtcNow.AddDays(_jwtSeetings.EmailValidationTokenExpiration);

		var claims = new List<Claim>
		{
		new Claim(JwtRegisteredClaimNames.Sub, sub),
								new Claim(ClaimTypes.Role, user.Role.ToString()), // FIXED: .ToString() to get string value
								new Claim(JwtRegisteredClaimNames.Email, user.Email),
								new Claim(JwtRegisteredClaimNames.Aud, _jwtSeetings.Audience),
								new Claim(JwtRegisteredClaimNames.Iss, _jwtSeetings.Issuer),
								new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString()),
								new Claim(JwtRegisteredClaimNames.Exp, expirationTime.ToString()),
								new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

		var jwtTokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(_jwtSeetings.Secret);

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
		var expirationTime = DateTime.UtcNow.AddDays(_jwtSeetings.DefaultExpiration);
		var claims = new List<Claim>
		{
								new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
								new Claim(ClaimTypes.Role, user.Role.ToString()), // FIXED: .ToString() to get string value
								new Claim(JwtRegisteredClaimNames.Email, user.Email),
								new Claim(JwtRegisteredClaimNames.Aud, _jwtSeetings.Audience),
								new Claim(JwtRegisteredClaimNames.Iss, _jwtSeetings.Issuer),
								new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString()),
								new Claim(JwtRegisteredClaimNames.Exp, expirationTime.ToString()),
								new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};
		var jwtTokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(_jwtSeetings.Secret);
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
		var key = Encoding.ASCII.GetBytes(_jwtSeetings.Secret);
		try
		{
			tokenHandler.ValidateToken(token, new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidIssuer = _jwtSeetings.Issuer,
				ValidAudience = _jwtSeetings.Audience,
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
				ValidIssuer = _jwtSeetings.Issuer,
				ValidAudience = _jwtSeetings.Audience,
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
		var key = Encoding.ASCII.GetBytes(_jwtSeetings.Secret);
		try
		{
			tokenHandler.ValidateToken(token, new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidIssuer = _jwtSeetings.Issuer,
				ValidAudience = _jwtSeetings.Audience,
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
		var key = Encoding.ASCII.GetBytes(_jwtSeetings.Secret);
		try
		{
			tokenHandler.ValidateToken(token, new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidIssuer = _jwtSeetings.Issuer,
				ValidAudience = _jwtSeetings.Audience,
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
		JwtSecurityToken securityToken = handler.ReadToken(token) as JwtSecurityToken;

		if(securityToken == null)
		{
			throw new Exception("Invalid Security token");
		}
		return securityToken.Claims;
	}

	public IEnumerable<Claim> DecodeToken(string token)
	{
		return GetClaims(token);
	}
}
