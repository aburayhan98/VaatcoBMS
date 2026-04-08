using FluentValidation;

namespace VaatcoBMS.Application.DTOs.User;
public class UserDtoValidator : AbstractValidator<UserDto>
{
	public UserDtoValidator() {
		RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
		RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100);
	}
}
public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
{
	public UpdateProfileDtoValidator() {
		RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
		RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100);
	}
}	