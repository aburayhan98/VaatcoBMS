using FluentValidation;

namespace VaatcoBMS.Application.DTOs.Invoice;

public class UpdateInvoiceDtoValidator : AbstractValidator<UpdateInvoiceDto>
{
	public UpdateInvoiceDtoValidator() {
		RuleFor(x => x).NotNull().WithMessage("Invoice data is required.");
		When(x => x != null, () => {
			RuleFor(x => x.Id).GreaterThan(0);
			RuleFor(x => x.CustomerId).GreaterThan(0);
			RuleFor(x => x.IssueDate).NotEmpty();
			RuleFor(x => x.DueDate).GreaterThanOrEqualTo(x => x.IssueDate).WithMessage("Due date must be on or after the invoice date.");
			RuleFor(x => x.TotalAmount).GreaterThan(0);
			RuleFor(x => x.InvoiceNumber).NotEmpty().MaximumLength(50);
			RuleFor(x => x.Status).MaximumLength(20);
			RuleFor(x => x.Notes).MaximumLength(1000);
		});
	}
}
