using Core.Entities;
using FluentValidation;

namespace Core.Validations.DamageReports;

public class DamageReportValidator : AbstractValidator<DamageReport>
{
    public DamageReportValidator()
    {
        RuleFor(x => x.RentalId)
            .NotEmpty().WithMessage("Rental is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.DamageCost)
            .GreaterThanOrEqualTo(0).WithMessage("Damage cost cannot be negative.");

        RuleFor(x => x.ReportedDate)
            .NotEmpty().WithMessage("Reported date is required.");

        RuleFor(x => x.PaidDate)
            .GreaterThanOrEqualTo(x => x.ReportedDate)
            .When(x => x.IsPaid && x.PaidDate.HasValue)
            .WithMessage("Paid date cannot be before reported date.");

        RuleFor(x => x.PaidDate)
            .NotNull()
            .When(x => x.IsPaid)
            .WithMessage("Paid date is required when damage is marked as paid.");
    }
}
