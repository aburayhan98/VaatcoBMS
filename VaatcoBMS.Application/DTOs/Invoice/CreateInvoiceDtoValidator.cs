using FluentValidation;

namespace VaatcoBMS.Application.DTOs.Invoice;

public class CreateInvoiceDtoValidator : AbstractValidator<CreateInvoiceDto>
{
	public CreateInvoiceDtoValidator() {
		RuleFor(x => x.CustomerId).GreaterThan(0);
		RuleFor(x => x.IssueDate).NotEmpty();
		RuleFor(x => x.DueDate).GreaterThanOrEqualTo(x => x.IssueDate).WithMessage("Due date must be on or after the invoice date.");
		RuleFor(x => x.TotalAmount).GreaterThan(0);
	}
}
