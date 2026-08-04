using Core.DTOs.Cars;
using FluentValidation;

namespace Core.Validations.Cars;

public class CompleteMaintenanceRequestValidator : AbstractValidator<CompleteMaintenanceRequest>
{
    public CompleteMaintenanceRequestValidator()
    {
        RuleFor(x => x.NextMaintenanceThresholdKm)
            .GreaterThan(0)
            .When(x => x.NextMaintenanceThresholdKm.HasValue)
            .WithMessage("Next maintenance threshold must be greater than 0.");
    }
}
