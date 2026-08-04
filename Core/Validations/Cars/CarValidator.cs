using Core.Entities;
using FluentValidation;

namespace Core.Validations.Cars;

public class CarValidator : AbstractValidator<Car>
{
    public CarValidator()
    {
        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Brand is required.")
            .MaximumLength(100).WithMessage("Brand must not exceed 100 characters.");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required.")
            .MaximumLength(100).WithMessage("Model must not exceed 100 characters.");

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
            .WithMessage($"Year must be between 1900 and {DateTime.UtcNow.Year + 1}.");

        RuleFor(x => x.CurrentMileage)
            .GreaterThanOrEqualTo(0).WithMessage("Current mileage cannot be negative.");

        RuleFor(x => x.MaintenanceThresholdKm)
            .GreaterThan(0).WithMessage("Maintenance threshold must be greater than 0.");

        RuleFor(x => x.DailyPrice)
            .GreaterThan(0).WithMessage("Daily price must be greater than 0.");

        RuleFor(x => x.ExtraKmFee)
            .GreaterThanOrEqualTo(0).WithMessage("Extra km fee cannot be negative.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status is not valid.");
    }
}
