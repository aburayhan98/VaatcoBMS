using FluentValidation;

namespace VaatcoBMS.Application.DTOs.InvoiceItem;
public class CreateInvoiceItemDtoValidator : AbstractValidator<CreateInvoiceItemDto>
{
	public CreateInvoiceItemDtoValidator() {
		RuleFor(x => x.InvoiceId).GreaterThan(0);
		RuleFor(x => x.ProductId).GreaterThan(0);
		RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be at least 1."); ;
		RuleFor(x => x.BonusQuantity).GreaterThanOrEqualTo(0);
		RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
	}
}
