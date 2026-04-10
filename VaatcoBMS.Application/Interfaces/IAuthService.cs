using VaatcoBMS.Application.DTOs.User;
using VaatcoBMS.Application.Model.Auth;

namespace VaatcoBMS.Application.Interfaces;

public interface IAuthService
{
	Task<TokenResponse> LoginAsync(LoginModel model);
	TokenResponse RefreshLogin(string refreshToken);
	Task<UserDto> RegisterAsync(Register model);
	Task<bool> VerifyEmailAsync(string token); // NEW
}