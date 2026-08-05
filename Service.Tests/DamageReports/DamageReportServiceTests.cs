using AutoMapper;
using Core.DTOs.DamageReports;
using Core.Entities;
using Core.Enums;
using Core.Exceptions;
using Core.Repositories;
using Core.UnitOfWork;
using Core.Validations.DamageReports;
using FluentAssertions;
using FluentValidation;
using Moq;
using Service.Mapping;
using Service.Services.DamageReports;

namespace Service.Tests.DamageReports;

public class DamageReportServiceTests
{
    private readonly Mock<IDamageReportRepository> _damageRepo = new();
    private readonly Mock<IRentalRepository> _rentalRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly IMapper _mapper;
    private readonly IValidator<CreateDamageReportRequest> _createValidator = new CreateDamageReportRequestValidator();
    private readonly DamageReportService _sut;

    public DamageReportServiceTests()
    {
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        _uow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        _sut = new DamageReportService(
            _damageRepo.Object,
            _uow.Object,
            _rentalRepo.Object,
            _mapper,
            _createValidator);
    }

    [Fact]
    public async Task CreateAsync_ActiveRental_CreatesUnpaidReport()
    {
        var userId = Guid.NewGuid();
        var rentalId = Guid.NewGuid();
        var rental = new Rental
        {
            Id = rentalId,
            UserId = userId,
            Status = RentalStatus.Active
        };

        _rentalRepo.Setup(x => x.GetByIdAsync(rentalId)).ReturnsAsync(rental);

        var request = new CreateDamageReportRequest
        {
            RentalId = rentalId,
            Description = "  Scratch on door  ",
            DamageCost = 500m
        };

        var result = await _sut.CreateAsync(userId, UserRole.Customer, request);

        result.Description.Should().Be("Scratch on door");
        result.DamageCost.Should().Be(500m);
        result.PaymentStatus.Should().Be(DamagePaymentStatus.Unpaid);
        result.PaidDate.Should().BeNull();
        _damageRepo.Verify(x => x.AddAsync(It.IsAny<DamageReport>()), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_OtherCustomersRental_ThrowsUnauthorized()
    {
        var rentalId = Guid.NewGuid();
        _rentalRepo.Setup(x => x.GetByIdAsync(rentalId)).ReturnsAsync(new Rental
        {
            Id = rentalId,
            UserId = Guid.NewGuid(),
            Status = RentalStatus.Active
        });

        var request = new CreateDamageReportRequest
        {
            RentalId = rentalId,
            Description = "Dent",
            DamageCost = 100m
        };

        var act = async () => await _sut.CreateAsync(Guid.NewGuid(), UserRole.Customer, request);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task CreateAsync_CompletedRental_ThrowsConflict()
    {
        var userId = Guid.NewGuid();
        var rentalId = Guid.NewGuid();
        _rentalRepo.Setup(x => x.GetByIdAsync(rentalId)).ReturnsAsync(new Rental
        {
            Id = rentalId,
            UserId = userId,
            Status = RentalStatus.Completed
        });

        var request = new CreateDamageReportRequest
        {
            RentalId = rentalId,
            Description = "Crack",
            DamageCost = 50m
        };

        var act = async () => await _sut.CreateAsync(userId, UserRole.Customer, request);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*active or extended*");
    }

    [Fact]
    public async Task CollectPaymentAsync_Unpaid_MarksPaid()
    {
        var reportId = Guid.NewGuid();
        var report = new DamageReport
        {
            Id = reportId,
            RentalId = Guid.NewGuid(),
            Description = "Scratch",
            DamageCost = 200m,
            PaymentStatus = DamagePaymentStatus.Unpaid,
            ReportedDate = DateTime.UtcNow.AddHours(-1)
        };

        _damageRepo.Setup(x => x.GetByIdAsync(reportId)).ReturnsAsync(report);

        var result = await _sut.CollectPaymentAsync(reportId);

        result.PaymentStatus.Should().Be(DamagePaymentStatus.Paid);
        result.PaidDate.Should().NotBeNull();
        _damageRepo.Verify(x => x.Update(report), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CollectPaymentAsync_AlreadyPaid_ThrowsConflict()
    {
        var reportId = Guid.NewGuid();
        _damageRepo.Setup(x => x.GetByIdAsync(reportId)).ReturnsAsync(new DamageReport
        {
            Id = reportId,
            Description = "Scratch",
            DamageCost = 200m,
            PaymentStatus = DamagePaymentStatus.Paid,
            ReportedDate = DateTime.UtcNow.AddDays(-1),
            PaidDate = DateTime.UtcNow.AddHours(-1)
        });

        var act = async () => await _sut.CollectPaymentAsync(reportId);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already been collected*");
    }
}
