using Core.Services;
using Core.Settings;

namespace Service.Rentals;

public class PricingService : IPricingService
{
    public int CalculateRentalDays(DateTime startDate, DateTime endDate)
    {
        var days = (endDate.Date - startDate.Date).Days;
        return Math.Max(1, days);
    }

    public decimal CalculateBasePrice(int rentalDays, decimal dailyPrice)
    {
        return rentalDays * dailyPrice;
    }

    public decimal CalculateLateFee(DateTime plannedEndDate, DateTime actualReturnDate, decimal dailyPrice)
    {
        var lateDays = (actualReturnDate.Date - plannedEndDate.Date).Days;
        if (lateDays <= 0)
        {
            return 0m;
        }

        return lateDays * dailyPrice;
    }

    public int CalculateIncludedKm(int rentalDays)
    {
        return rentalDays * RentalPricingRules.IncludedKmPerDay;
    }

    public decimal CalculateExtraKmCharge(int startMileage, int endMileage, int includedKm, decimal extraKmFee)
    {
        var drivenKm = endMileage - startMileage;
        var extraKm = drivenKm - includedKm;
        if (extraKm <= 0)
        {
            return 0m;
        }

        return extraKm * extraKmFee;
    }
}
