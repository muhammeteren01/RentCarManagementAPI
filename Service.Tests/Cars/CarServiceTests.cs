using System.Linq.Expressions;
using AutoMapper;
using Core.DTOs.Cars;
using Core.Entities;
using Core.Enums;
using Core.Exceptions;
using Core.Repositories;
using Core.UnitOfWork;
using Core.Validations.Cars;
using FluentAssertions;
using Moq;
using Service.Mapping;
using Service.Services.Cars;

namespace Service.Tests.Cars;

public class CarServiceTests
{
    private readonly Mock<ICarRepository> _carRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly IMapper _mapper;
    private readonly CarService _sut;

    public CarServiceTests()
    {
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        _uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _sut = new CarService(
            _carRepo.Object,
            _uow.Object,
            _mapper,
            new CreateCarRequestValidator(),
            new UpdateCarRequestValidator(),
            new CompleteMaintenanceRequestValidator());
    }

    [Fact]
    public async Task GetCarsAsync_Customer_ReturnsOnlyAvailable()
    {
        var available = new List<Car>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2024,
                Status = CarStatus.Available,
                DailyPrice = 100m
            }
        };

        _carRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Car, bool>>>()))
            .ReturnsAsync(available);

        var result = (await _sut.GetCarsAsync(UserRole.Customer)).ToList();

        result.Should().HaveCount(1);
        result[0].Brand.Should().Be("Toyota");
        _carRepo.Verify(x => x.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesCar()
    {
        var request = new CreateCarRequest
        {
            Brand = "Toyota",
            Model = "Corolla",
            Year = 2024,
            CurrentMileage = 1000,
            MaintenanceThresholdKm = 10000,
            DailyPrice = 150m,
            ExtraKmFee = 2m,
            Status = CarStatus.Available
        };

        var result = await _sut.CreateAsync(request);

        result.Brand.Should().Be("Toyota");
        result.DailyPrice.Should().Be(150m);
        _carRepo.Verify(x => x.AddAsync(It.IsAny<Car>()), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_RentedCar_ThrowsConflict()
    {
        var carId = Guid.NewGuid();
        _carRepo.Setup(x => x.GetByIdAsync(carId)).ReturnsAsync(new Car
        {
            Id = carId,
            Brand = "Ford",
            Model = "Focus",
            Year = 2020,
            Status = CarStatus.Rented,
            DailyPrice = 80m
        });

        var act = async () => await _sut.DeleteAsync(carId);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*currently rented*");
    }

    [Fact]
    public async Task SendToMaintenanceAsync_AvailableCar_SetsMaintenance()
    {
        var carId = Guid.NewGuid();
        var car = new Car
        {
            Id = carId,
            Brand = "Honda",
            Model = "Civic",
            Year = 2022,
            Status = CarStatus.Available,
            DailyPrice = 90m
        };
        _carRepo.Setup(x => x.GetByIdAsync(carId)).ReturnsAsync(car);

        var result = await _sut.SendToMaintenanceAsync(carId);

        result.Status.Should().Be(CarStatus.Maintenance);
        car.Status.Should().Be(CarStatus.Maintenance);
    }

    [Fact]
    public async Task SendToMaintenanceAsync_RentedCar_ThrowsConflict()
    {
        var carId = Guid.NewGuid();
        _carRepo.Setup(x => x.GetByIdAsync(carId)).ReturnsAsync(new Car
        {
            Id = carId,
            Brand = "BMW",
            Model = "320i",
            Year = 2021,
            Status = CarStatus.Rented,
            DailyPrice = 200m
        });

        var act = async () => await _sut.SendToMaintenanceAsync(carId);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*rented car*");
    }

    [Fact]
    public async Task CompleteMaintenanceAsync_InMaintenance_SetsAvailableAndThreshold()
    {
        var carId = Guid.NewGuid();
        var car = new Car
        {
            Id = carId,
            Brand = "Audi",
            Model = "A3",
            Year = 2023,
            CurrentMileage = 20000,
            MaintenanceThresholdKm = 15000,
            Status = CarStatus.Maintenance,
            DailyPrice = 180m
        };
        _carRepo.Setup(x => x.GetByIdAsync(carId)).ReturnsAsync(car);

        var result = await _sut.CompleteMaintenanceAsync(carId, new CompleteMaintenanceRequest
        {
            NextMaintenanceThresholdKm = 30000
        });

        result.Status.Should().Be(CarStatus.Available);
        car.MaintenanceThresholdKm.Should().Be(30000);
    }

    [Fact]
    public async Task CompleteMaintenanceAsync_NotInMaintenance_ThrowsConflict()
    {
        var carId = Guid.NewGuid();
        _carRepo.Setup(x => x.GetByIdAsync(carId)).ReturnsAsync(new Car
        {
            Id = carId,
            Brand = "Audi",
            Model = "A3",
            Year = 2023,
            CurrentMileage = 10000,
            Status = CarStatus.Available,
            DailyPrice = 180m
        });

        var act = async () => await _sut.CompleteMaintenanceAsync(carId, new CompleteMaintenanceRequest());

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*Only cars in maintenance*");
    }
}
