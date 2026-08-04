using Core.Entities;
using FluentValidation;

namespace Core.Validations.Rentals;

public class RentalValidator : AbstractValidator<Rental>
{
    public RentalValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User is required.");

        RuleFor(x => x.CarId)
            .NotEmpty().WithMessage("Car is required.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.PlannedEndDate)
            .NotEmpty().WithMessage("Planned end date is required.")
            .GreaterThan(x => x.StartDate).WithMessage("Planned end date must be after start date.");

        RuleFor(x => x.ActualReturnDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.ActualReturnDate.HasValue)
            .WithMessage("Actual return date cannot be before start date.");

        RuleFor(x => x.StartMileage)
            .GreaterThanOrEqualTo(0).WithMessage("Start mileage cannot be negative.");

        RuleFor(x => x.EndMileage)
            .GreaterThanOrEqualTo(x => x.StartMileage)
            .When(x => x.EndMileage.HasValue)
            .WithMessage("End mileage cannot be less than start mileage.");

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Base price cannot be negative.");

        RuleFor(x => x.LateFee)
            .GreaterThanOrEqualTo(0).WithMessage("Late fee cannot be negative.");

        RuleFor(x => x.ExtraKmCharge)
            .GreaterThanOrEqualTo(0).WithMessage("Extra km charge cannot be negative.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status is not valid.");
    }
}
