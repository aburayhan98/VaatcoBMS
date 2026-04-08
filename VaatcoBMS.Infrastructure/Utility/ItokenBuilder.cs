namespace VaatcoBMS.Infrastructure.Utility;

public interface ITokenBuilder
{
    // For JWT Login Tokens
    string BuildToken(string email, int userId, string role);
    
    // Your existing email token method
    string BuildEmailToken(); // (Update signature to match what you already have)
}
