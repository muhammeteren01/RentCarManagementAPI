using Core.DTOs.DamageReports;
using FluentValidation;

namespace Core.Validations.DamageReports;

public class CreateDamageReportRequestValidator : AbstractValidator<CreateDamageReportRequest>
{
    public CreateDamageReportRequestValidator()
    {
        RuleFor(x => x.RentalId)
            .NotEmpty().WithMessage("Rental is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");
    }
}
