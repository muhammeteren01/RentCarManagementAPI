using Core.DTOs.Rentals;
using FluentValidation;

namespace Core.Validations.Rentals;

public class ReturnRentalRequestValidator : AbstractValidator<ReturnRentalRequest>
{
    public ReturnRentalRequestValidator()
    {
        RuleFor(x => x.EndMileage)
            .GreaterThanOrEqualTo(0).WithMessage("End mileage cannot be negative.");
    }
}
