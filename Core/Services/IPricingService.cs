namespace Core.Services;

public interface IPricingService
{
    int CalculateRentalDays(DateTime startDate, DateTime endDate);
    decimal CalculateBasePrice(int rentalDays, decimal dailyPrice);
    decimal CalculateLateFee(DateTime plannedEndDate, DateTime actualReturnDate, decimal dailyPrice);
    int CalculateIncludedKm(int rentalDays);
    decimal CalculateExtraKmCharge(int startMileage, int endMileage, int includedKm, decimal extraKmFee);
}
