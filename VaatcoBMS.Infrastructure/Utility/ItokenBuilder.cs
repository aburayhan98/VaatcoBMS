using System.Security.Claims;
using VaatcoBMS.Domain.Entities;

namespace VaatcoBMS.Infrastructure.Utility;

public interface ITokenBuilder
{
    // For JWT Login Tokens
    string BuildToken(string email, int userId, string role);
    
    // Your existing email token method
    string BuildEmailToken(User user, string purpose);
    bool IsJwtValid(string token);
    IEnumerable<Claim> GetClaims(string token);
}
