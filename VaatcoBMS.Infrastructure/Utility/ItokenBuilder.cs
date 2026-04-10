using System.Security.Claims;
using VaatcoBMS.Application;
using VaatcoBMS.Domain.Entities;

namespace VaatcoBMS.Infrastructure.Utility;

public interface ITokenBuilder
{
    // Now returns a TokenResponse (Access + Refresh tokens)
    TokenResponse BuildTokens(string email, int userId, string name, string role);
    TokenResponse RefreshTokens(string refreshToken);
    
    // Email token logic we added
    string BuildEmailToken(User user, string purpose);
    bool IsJwtValid(string token);
    IEnumerable<Claim> GetClaims(string token);
}
