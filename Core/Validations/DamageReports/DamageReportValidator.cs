using Core.Entities;
using Core.Enums;
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

        RuleFor(x => x.PaymentStatus)
            .IsInEnum().WithMessage("Payment status is invalid.");

        RuleFor(x => x.PaidDate)
            .GreaterThanOrEqualTo(x => x.ReportedDate)
            .When(x => x.PaymentStatus == DamagePaymentStatus.Paid && x.PaidDate.HasValue)
            .WithMessage("Paid date cannot be before reported date.");

        RuleFor(x => x.PaidDate)
            .NotNull()
            .When(x => x.PaymentStatus == DamagePaymentStatus.Paid)
            .WithMessage("Paid date is required when damage is marked as paid.");

        RuleFor(x => x.PaidDate)
            .Null()
            .When(x => x.PaymentStatus == DamagePaymentStatus.Unpaid)
            .WithMessage("Paid date must be empty when damage is unpaid.");
    }
}
