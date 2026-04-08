using FluentValidation;

namespace VaatcoBMS.Application.DTOs.Customer;

public class UpdateCustomerDtoValidator : AbstractValidator<UpdateCustomerDto>
{
	public UpdateCustomerDtoValidator() {
		RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
		RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
		RuleFor(x => x.Phone).NotEmpty().MaximumLength(30).Matches(@"^\+?[0-9]{10,15}$").WithMessage("Phone must be 10-15 digits."); 
		RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
	}
}
