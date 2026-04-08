using VaatcoBMS.Application.DTOs.User;
using VaatcoBMS.Application.Model.Auth;

namespace VaatcoBMS.Application.Interfaces;

public interface IAuthService
{
    Task<string> LoginAsync(LoginModel model);
    Task<UserDto> RegisterAsync(Register model);
}