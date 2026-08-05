using AutoMapper;
using Core.DTOs.Rentals;
using Core.Entities;
using Core.Enums;
using Core.Exceptions;
using Core.Repositories;
using Core.UnitOfWork;
using Core.Validations.Rentals;
using FluentAssertions;
using FluentValidation;
using Moq;
using Service.Mapping;
using Service.Services.Rentals;

namespace Service.Tests.Rentals;

public class RentalServiceTests
{
    private readonly Mock<IRentalRepository> _rentalRepo = new();
    private readonly Mock<ICarRepository> _carRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly PricingService _pricing = new();
    private readonly IMapper _mapper;
    private readonly RentalService _sut;

    public RentalServiceTests()
    {
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        _sut = new RentalService(
            _rentalRepo.Object,
            _uow.Object,
            _carRepo.Object,
            _pricing,
            _mapper,
            new CreateRentalRequestValidator(),
            new ExtendRentalRequestValidator(),
            new ReturnRentalRequestValidator());
    }

    [Fact]
    public async Task CreateAsync_AvailableCar_CreatesActiveRentalAndMarksCarRented()
    {
        var userId = Guid.NewGuid();
        var carId = Guid.NewGuid();
        var car = new Car
        {
            Id = carId,
            Brand = "Toyota",
            Model = "Corolla",
            DailyPrice = 100m,
            ExtraKmFee = 2m,
            CurrentMileage = 1000,
            Status = CarStatus.Available
        };

        _carRepo.Setup(x => x.GetByIdAsync(carId)).ReturnsAsync(car);

        var request = new CreateRentalRequest
        {
            CarId = carId,
            StartDate = new DateTime(2026, 8, 1),
            PlannedEndDate = new DateTime(2026, 8, 4)
        };

        var result = await _sut.CreateAsync(userId, request);

        result.Status.Should().Be(RentalStatus.Active);
        result.BasePrice.Should().Be(300m);
        result.CarBrand.Should().Be("Toyota");
        car.Status.Should().Be(CarStatus.Rented);
        _rentalRepo.Verify(x => x.AddAsync(It.IsAny<Rental>()), Times.Once);
        _carRepo.Verify(x => x.Update(car), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CarNotAvailable_ThrowsConflict()
    {
        var carId = Guid.NewGuid();
        _carRepo.Setup(x => x.GetByIdAsync(carId)).ReturnsAsync(new Car
        {
            Id = carId,
            Brand = "Ford",
            Model = "Focus",
            DailyPrice = 80m,
            Status = CarStatus.Rented
        });

        var request = new CreateRentalRequest
        {
            CarId = carId,
            StartDate = new DateTime(2026, 8, 1),
            PlannedEndDate = new DateTime(2026, 8, 2)
        };

        var act = async () => await _sut.CreateAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*not available*");
    }

    [Fact]
    public async Task CancelAsync_ActiveRental_CancelsAndFreesCar()
    {
        var userId = Guid.NewGuid();
        var rentalId = Guid.NewGuid();
        var carId = Guid.NewGuid();

        var rental = new Rental
        {
            Id = rentalId,
            UserId = userId,
            CarId = carId,
            StartDate = new DateTime(2026, 8, 1),
            PlannedEndDate = new DateTime(2026, 8, 3),
            StartMileage = 1000,
            BasePrice = 200m,
            Status = RentalStatus.Active
        };

        var car = new Car
        {
            Id = carId,
            Brand = "Honda",
            Model = "Civic",
            DailyPrice = 100m,
            Status = CarStatus.Rented
        };

        _rentalRepo.Setup(x => x.GetByIdAsync(rentalId)).ReturnsAsync(rental);
        _carRepo.Setup(x => x.GetByIdAsync(carId)).ReturnsAsync(car);

        var result = await _sut.CancelAsync(rentalId, userId, UserRole.Customer);

        result.Status.Should().Be(RentalStatus.Cancelled);
        car.Status.Should().Be(CarStatus.Available);
        _rentalRepo.Verify(x => x.Update(rental), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_CompletedRental_ThrowsConflict()
    {
        var userId = Guid.NewGuid();
        var rentalId = Guid.NewGuid();
        _rentalRepo.Setup(x => x.GetByIdAsync(rentalId)).ReturnsAsync(new Rental
        {
            Id = rentalId,
            UserId = userId,
            CarId = Guid.NewGuid(),
            StartDate = new DateTime(2026, 8, 1),
            PlannedEndDate = new DateTime(2026, 8, 2),
            Status = RentalStatus.Completed
        });

        var act = async () => await _sut.CancelAsync(rentalId, userId, UserRole.Customer);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*cannot be cancelled*");
    }

    [Fact]
    public async Task ReturnAsync_OnTimeWithExtraKm_CalculatesFeesAndCompletes()
    {
        var userId = Guid.NewGuid();
        var rentalId = Guid.NewGuid();
        var carId = Guid.NewGuid();

        var rental = new Rental
        {
            Id = rentalId,
            UserId = userId,
            CarId = carId,
            StartDate = new DateTime(2026, 8, 1),
            PlannedEndDate = new DateTime(2026, 8, 3), // 2 days, included km = 400
            StartMileage = 1000,
            BasePrice = 200m,
            Status = RentalStatus.Active
        };

        var car = new Car
        {
            Id = carId,
            Brand = "BMW",
            Model = "320i",
            DailyPrice = 100m,
            ExtraKmFee = 2m,
            CurrentMileage = 1000,
            Status = CarStatus.Rented
        };

        _rentalRepo.Setup(x => x.GetByIdAsync(rentalId)).ReturnsAsync(rental);
        _carRepo.Setup(x => x.GetByIdAsync(carId)).ReturnsAsync(car);

        var request = new ReturnRentalRequest
        {
            EndMileage = 1500, // driven 500 => extra 100 * 2 = 200
            ActualReturnDate = new DateTime(2026, 8, 3)
        };

        var result = await _sut.ReturnAsync(rentalId, userId, UserRole.Customer, request);

        result.Status.Should().Be(RentalStatus.Completed);
        result.LateFee.Should().Be(0m);
        result.ExtraKmCharge.Should().Be(200m);
        result.TotalPrice.Should().Be(400m); // 200 + 0 + 200
        car.Status.Should().Be(CarStatus.Available);
        car.CurrentMileage.Should().Be(1500);
    }
}
