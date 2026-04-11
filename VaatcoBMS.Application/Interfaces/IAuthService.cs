using VaatcoBMS.Application.DTOs.User;
using VaatcoBMS.Application.Model.Auth;

namespace VaatcoBMS.Application.Interfaces;

public interface IAuthService
{
	Task<TokenResponse> LoginAsync(LoginModel model);
	TokenResponse RefreshLogin(string refreshToken);
	//Task<UserDto> RegisterAsync(Register model);
	Task<string> RegisterAsync(Register model);
	Task<bool> VerifyEmailAsync(string token);
	Task ForgotPasswordAsync(string email);
	Task ResetPasswordAsync(ResetPasswordModel model);

	// NEW: Admin manually creates an account (skips email verify + approval)
	Task<UserDto> CreateUserByAdminAsync(Register model, int createdByUserId);
}