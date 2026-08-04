using Core.DTOs.Rentals;
using FluentValidation;

namespace Core.Validations.Rentals;

public class ExtendRentalRequestValidator : AbstractValidator<ExtendRentalRequest>
{
    public ExtendRentalRequestValidator()
    {
        RuleFor(x => x.NewPlannedEndDate)
            .NotEmpty().WithMessage("New planned end date is required.");
    }
}
