
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using VaatcoBMS.Application;
//using VaatcoBMS.Application.Configs;

//namespace VaatcoBMS.Infrastructure.Services;

///// <summary>
///// 
///// </summary>
///// <param name="tokenConfig"></param>
//public class TokenService(TokenConfig tokenConfig)
//{
//	private const string RefreshTokenClaimType = "r:id";
//	private readonly TokenConfig _tokenConfig = tokenConfig;

//	/// <summary>
//	/// 
//	/// </summary>
//	/// <param name="userId"></param>
//	/// <param name="companyId"></param>
//	/// <returns></returns>
//	public TokenResponse Create(int userId, string name, string email)
//	{
//		var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenConfig.SecretKey));

//		var handler = new JwtSecurityTokenHandler();
//		var claims = new List<Claim>
//						{
//								new(ClaimTypes.NameIdentifier, userId.ToString()),
//								new(ClaimTypes.Name, name),
//								new(ClaimTypes.Email, email),
//						};

//		var expires = DateTime.Now.AddHours(_tokenConfig.ExpiryInHours);

//		var token = handler.CreateJwtSecurityToken(
//				_tokenConfig.Issuer,
//				_tokenConfig.Audience,
//				new ClaimsIdentity(claims),
//				DateTime.Now,
//				expires,
//				DateTime.Now,
//				new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

//		var refreshTokenClaims = new List<Claim>()
//						{
//								new(RefreshTokenClaimType, userId.ToString()),
//								new(ClaimTypes.Name, name),
//						};

//		var refreshToken = handler.CreateJwtSecurityToken(
//				_tokenConfig.Issuer,
//				_tokenConfig.Audience,
//				new ClaimsIdentity(refreshTokenClaims),
//				DateTime.Now,
//				DateTime.Now.AddDays(7),
//				DateTime.Now,
//				new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

//		return new TokenResponse()
//		{
//			AccessToken = handler.WriteToken(token),
//			ExpiresAtUtc = expires,
//			RefreshToken = handler.WriteToken(refreshToken)
//		};
//	}

//	/// <summary>
//	/// 
//	/// </summary>
//	/// <param name="refreshToken"></param>
//	/// <returns></returns>
//	public TokenResponse Refresh(string refreshToken)
//	{
//		try
//		{
//			var handler = new JwtSecurityTokenHandler();
//			var result = handler.ValidateToken(refreshToken, new TokenValidationParameters()
//			{
//				ValidateAudience = true,
//				ValidateIssuer = true,
//				ValidateLifetime = true,
//				ValidateIssuerSigningKey = true,
//				ValidAudience = _tokenConfig.Audience,
//				ValidIssuer = _tokenConfig.Issuer,
//				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenConfig.SecretKey))
//			}, out _);

//			if (result == null) return null;

//			var userIdClaim = result.Claims.FirstOrDefault(c => c.Type == RefreshTokenClaimType);
//			if (userIdClaim == null) return null;

//			var userNameClaim = result.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
//			if (userNameClaim == null) return null;

//			var userEmailClaim = result.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
//			if (userEmailClaim == null) return null;

//			return Create(int.Parse(userIdClaim.Value), userNameClaim.Value, userEmailClaim.Value);
//		}
//		catch
//		{
//			return null;
//		}
//	}
//}
