using VaatcoBMS.Application.DTOs.User;
using VaatcoBMS.Domain.Enums;

namespace VaatcoBMS.Application.Interfaces;

public interface IUserService 
{ 
	Task<IEnumerable<UserDto>> GetAllAsync(); 
	Task<IEnumerable<UserDto>> GetPendingApprovalAsync();
	Task<UserDto?> GetByIdAsync(int id);
	Task<UserDto?> GetByEmailAsync(string email);
	Task<UserDto> UpdateProfileAsync(int id, UpdateProfileDto dto);
	Task ChangeRoleAsync(int id, UserRole newRole);
	Task ApproveUserAsync(int id);
	Task RejectUserAsync(int id);
	Task ChangePasswordAsync(int id, ChangePasswordDto dto);
	Task DeactivateAsync(int id);
	Task<bool> EmailExistsAsync(string email, int? excludeId = null);
}
