using Core.Settings;
using FluentAssertions;
using Service.Services.Rentals;

namespace Service.Tests.Rentals;

public class PricingServiceTests
{
    private readonly PricingService _sut = new();

    [Fact]
    public void CalculateRentalDays_SameDay_ReturnsOne()
    {
        var start = new DateTime(2026, 8, 1);
        var end = new DateTime(2026, 8, 1);

        _sut.CalculateRentalDays(start, end).Should().Be(1);
    }

    [Fact]
    public void CalculateRentalDays_ThreeCalendarDaysDiff_ReturnsThree()
    {
        var start = new DateTime(2026, 8, 1);
        var end = new DateTime(2026, 8, 4);

        _sut.CalculateRentalDays(start, end).Should().Be(3);
    }

    [Fact]
    public void CalculateBasePrice_MultipliesDaysByDailyPrice()
    {
        _sut.CalculateBasePrice(3, 100m).Should().Be(300m);
    }

    [Fact]
    public void CalculateLateFee_OnTime_ReturnsZero()
    {
        var planned = new DateTime(2026, 8, 5);
        var actual = new DateTime(2026, 8, 5);

        _sut.CalculateLateFee(planned, actual, 100m).Should().Be(0m);
    }

    [Fact]
    public void CalculateLateFee_TwoDaysLate_ReturnsTwoDaysFee()
    {
        var planned = new DateTime(2026, 8, 5);
        var actual = new DateTime(2026, 8, 7);

        _sut.CalculateLateFee(planned, actual, 100m).Should().Be(200m);
    }

    [Fact]
    public void CalculateIncludedKm_UsesDailyRule()
    {
        _sut.CalculateIncludedKm(3).Should().Be(3 * RentalPricingRules.IncludedKmPerDay);
    }

    [Fact]
    public void CalculateExtraKmCharge_WithinIncluded_ReturnsZero()
    {
        _sut.CalculateExtraKmCharge(1000, 1100, includedKm: 200, extraKmFee: 2m)
            .Should().Be(0m);
    }

    [Fact]
    public void CalculateExtraKmCharge_OverIncluded_ChargesExtraKm()
    {
        // driven 300, included 200 => 100 * 2 = 200
        _sut.CalculateExtraKmCharge(1000, 1300, includedKm: 200, extraKmFee: 2m)
            .Should().Be(200m);
    }
}
