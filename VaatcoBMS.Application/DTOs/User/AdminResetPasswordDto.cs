using System.ComponentModel.DataAnnotations;

namespace VaatcoBMS.Application.DTOs.User;

public class AdminResetPasswordDto
{
  [Required]
  [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
  public string NewPassword { get; set; } = string.Empty;
}