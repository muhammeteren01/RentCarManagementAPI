using Core.DTOs.Rentals;
using FluentValidation;

namespace Core.Validations.Rentals;

public class CreateRentalRequestValidator : AbstractValidator<CreateRentalRequest>
{
    public CreateRentalRequestValidator()
    {
        RuleFor(x => x.CarId)
            .NotEmpty().WithMessage("Car is required.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.PlannedEndDate)
            .NotEmpty().WithMessage("Planned end date is required.")
            .GreaterThan(x => x.StartDate).WithMessage("Planned end date must be after start date.");
    }
}
