using AutoMapper;
using Core.DTOs.DamageReports;
using Core.Entities;
using Core.Enums;
using Core.Exceptions;
using Core.Repositories;
using Core.Services;
using Core.UnitOfWork;
using FluentValidation;
using Service.Services.Common;
using ValidationException = Core.Validations.ValidationException;

namespace Service.Services.DamageReports;

public class DamageReportService : GenericService<DamageReport>, IDamageReportService
{
    private readonly IDamageReportRepository _damageReportRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateDamageReportRequest> _createValidator;

    public DamageReportService(
        IDamageReportRepository repository,
        IUnitOfWork unitOfWork,
        IRentalRepository rentalRepository,
        IMapper mapper,
        IValidator<CreateDamageReportRequest> createValidator)
        : base(repository, unitOfWork)
    {
        _damageReportRepository = repository;
        _rentalRepository = rentalRepository;
        _mapper = mapper;
        _createValidator = createValidator;
    }

    public async Task<DamageReportResponse> CreateAsync(
        Guid requesterId,
        UserRole role,
        CreateDamageReportRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var rental = await _rentalRepository.GetByIdAsync(request.RentalId)
            ?? throw new NotFoundException($"Rental with id '{request.RentalId}' was not found.");

        if (role != UserRole.Admin && rental.UserId != requesterId)
        {
            throw new UnauthorizedException("You are not allowed to create a damage report for this rental.");
        }

        if (rental.Status is not (RentalStatus.Active or RentalStatus.Extended))
        {
            throw new ConflictException("Damage reports can only be created for active or extended rentals.");
        }

        var report = new DamageReport
        {
            Id = Guid.NewGuid(),
            RentalId = rental.Id,
            Description = request.Description.Trim(),
            DamageCost = request.DamageCost,
            PaymentStatus = DamagePaymentStatus.Unpaid,
            ReportedDate = DateTime.UtcNow,
            PaidDate = null
        };

        await _damageReportRepository.AddAsync(report);
        await UnitOfWork.SaveChangesAsync();

        return _mapper.Map<DamageReportResponse>(report);
    }

    public async Task<DamageReportResponse> CollectPaymentAsync(Guid damageReportId)
    {
        var report = await _damageReportRepository.GetByIdAsync(damageReportId)
            ?? throw new NotFoundException($"Damage report with id '{damageReportId}' was not found.");

        if (report.PaymentStatus == DamagePaymentStatus.Paid)
        {
            throw new ConflictException("Damage report payment has already been collected.");
        }

        report.PaymentStatus = DamagePaymentStatus.Paid;
        report.PaidDate = DateTime.UtcNow;

        _damageReportRepository.Update(report);
        await UnitOfWork.SaveChangesAsync();

        return _mapper.Map<DamageReportResponse>(report);
    }
}
