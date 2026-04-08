using FluentValidation;

namespace VaatcoBMS.Application.DTOs.Product;

public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
	public UpdateProductDtoValidator()
	{
		RuleFor(x => x.Id).GreaterThan(0);
		RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(30);
		RuleFor(x => x.ProductName).NotEmpty().MaximumLength(200);
		RuleFor(x => x.PackSize).NotEmpty().MaximumLength(50);
		RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than zero.");
		RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
	}
}
