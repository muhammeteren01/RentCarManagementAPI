using AutoMapper;
using Core.DTOs.Rentals;
using Core.Entities;
using Core.Enums;
using Core.Exceptions;
using Core.Repositories;
using Core.Services;
using Core.UnitOfWork;
using Service.Services.Common;
using FluentValidation;
using ValidationException = Core.Validations.ValidationException;

namespace Service.Services.Rentals;

public class RentalService : GenericService<Rental>, IRentalService
{
    private readonly IRentalRepository _rentalRepository;
    private readonly ICarRepository _carRepository;
    private readonly IPricingService _pricingService;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateRentalRequest> _createValidator;
    private readonly IValidator<ExtendRentalRequest> _extendValidator;
    private readonly IValidator<ReturnRentalRequest> _returnValidator;

    public RentalService(
        IRentalRepository repository,
        IUnitOfWork unitOfWork,
        ICarRepository carRepository,
        IPricingService pricingService,
        IMapper mapper,
        IValidator<CreateRentalRequest> createValidator,
        IValidator<ExtendRentalRequest> extendValidator,
        IValidator<ReturnRentalRequest> returnValidator)
        : base(repository, unitOfWork)
    {
        _rentalRepository = repository;
        _carRepository = carRepository;
        _pricingService = pricingService;
        _mapper = mapper;
        _createValidator = createValidator;
        _extendValidator = extendValidator;
        _returnValidator = returnValidator;
    }

    public async Task<RentalResponse> CreateAsync(Guid userId, CreateRentalRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var car = await _carRepository.GetByIdAsync(request.CarId)
            ?? throw new NotFoundException($"Car with id '{request.CarId}' was not found.");

        if (car.Status != CarStatus.Available)
        {
            throw new ConflictException("Car is not available for rental.");
        }

        var rentalDays = _pricingService.CalculateRentalDays(request.StartDate, request.PlannedEndDate);
        var basePrice = _pricingService.CalculateBasePrice(rentalDays, car.DailyPrice);

        var rental = new Rental
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CarId = car.Id,
            StartDate = request.StartDate,
            PlannedEndDate = request.PlannedEndDate,
            StartMileage = car.CurrentMileage,
            BasePrice = basePrice,
            LateFee = 0,
            ExtraKmCharge = 0,
            Status = RentalStatus.Active
        };

        car.Status = CarStatus.Rented;

        await _rentalRepository.AddAsync(rental);
        _carRepository.Update(car);
        await UnitOfWork.SaveChangesAsync();

        return MapToResponse(rental, car);
    }

    public async Task<RentalResponse> ExtendAsync(
        Guid rentalId,
        Guid requesterId,
        UserRole role,
        ExtendRentalRequest request)
    {
        var validationResult = await _extendValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var rental = await GetAccessibleRentalAsync(rentalId, requesterId, role);
        EnsureActiveOrExtended(rental);

        if (request.NewPlannedEndDate <= rental.PlannedEndDate)
        {
            throw new ConflictException("New planned end date must be after current planned end date.");
        }

        if (request.NewPlannedEndDate <= rental.StartDate)
        {
            throw new ConflictException("New planned end date must be after start date.");
        }

        var car = await _carRepository.GetByIdAsync(rental.CarId)
            ?? throw new NotFoundException($"Car with id '{rental.CarId}' was not found.");

        rental.PlannedEndDate = request.NewPlannedEndDate;
        rental.Status = RentalStatus.Extended;

        var rentalDays = _pricingService.CalculateRentalDays(rental.StartDate, rental.PlannedEndDate);
        rental.BasePrice = _pricingService.CalculateBasePrice(rentalDays, car.DailyPrice);

        _rentalRepository.Update(rental);
        await UnitOfWork.SaveChangesAsync();

        return MapToResponse(rental, car);
    }

    public async Task<RentalResponse> ReturnAsync(
        Guid rentalId,
        Guid requesterId,
        UserRole role,
        ReturnRentalRequest request)
    {
        var validationResult = await _returnValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var rental = await GetAccessibleRentalAsync(rentalId, requesterId, role);
        EnsureActiveOrExtended(rental);

        if (request.EndMileage < rental.StartMileage)
        {
            throw new ConflictException("End mileage cannot be less than start mileage.");
        }

        var car = await _carRepository.GetByIdAsync(rental.CarId)
            ?? throw new NotFoundException($"Car with id '{rental.CarId}' was not found.");

        var actualReturnDate = request.ActualReturnDate ?? DateTime.UtcNow;
        if (actualReturnDate < rental.StartDate)
        {
            throw new ConflictException("Actual return date cannot be before start date.");
        }

        var rentalDays = _pricingService.CalculateRentalDays(rental.StartDate, rental.PlannedEndDate);
        var includedKm = _pricingService.CalculateIncludedKm(rentalDays);

        rental.ActualReturnDate = actualReturnDate;
        rental.EndMileage = request.EndMileage;
        rental.LateFee = _pricingService.CalculateLateFee(rental.PlannedEndDate, actualReturnDate, car.DailyPrice);
        rental.ExtraKmCharge = _pricingService.CalculateExtraKmCharge(
            rental.StartMileage,
            request.EndMileage,
            includedKm,
            car.ExtraKmFee);
        rental.Status = RentalStatus.Completed;

        car.CurrentMileage = request.EndMileage;
        car.Status = CarStatus.Available;

        _rentalRepository.Update(rental);
        _carRepository.Update(car);
        await UnitOfWork.SaveChangesAsync();

        return MapToResponse(rental, car);
    }

    public async Task<RentalResponse> GetByIdAsync(Guid rentalId, Guid requesterId, UserRole role)
    {
        var rental = await GetAccessibleRentalAsync(rentalId, requesterId, role);
        var car = await _carRepository.GetByIdAsync(rental.CarId);
        return MapToResponse(rental, car);
    }

    public async Task<IEnumerable<RentalResponse>> GetHistoryAsync(Guid requesterId, UserRole role)
    {
        IEnumerable<Rental> rentals = role == UserRole.Admin
            ? await _rentalRepository.GetAllAsync()
            : await _rentalRepository.FindAsync(r => r.UserId == requesterId);

        var result = new List<RentalResponse>();
        foreach (var rental in rentals.OrderByDescending(r => r.StartDate))
        {
            var car = await _carRepository.GetByIdAsync(rental.CarId);
            result.Add(MapToResponse(rental, car));
        }

        return result;
    }

    private async Task<Rental> GetAccessibleRentalAsync(Guid rentalId, Guid requesterId, UserRole role)
    {
        var rental = await _rentalRepository.GetByIdAsync(rentalId)
            ?? throw new NotFoundException($"Rental with id '{rentalId}' was not found.");

        if (role != UserRole.Admin && rental.UserId != requesterId)
        {
            throw new UnauthorizedException("You are not allowed to access this rental.");
        }

        return rental;
    }

    private static void EnsureActiveOrExtended(Rental rental)
    {
        if (rental.Status is not (RentalStatus.Active or RentalStatus.Extended))
        {
            throw new ConflictException("Only active or extended rentals can be modified.");
        }
    }

    private RentalResponse MapToResponse(Rental rental, Car? car)
    {
        var rentalDays = _pricingService.CalculateRentalDays(rental.StartDate, rental.PlannedEndDate);
        var includedKm = _pricingService.CalculateIncludedKm(rentalDays);

        var response = _mapper.Map<RentalResponse>(rental);
        response.RentalDays = rentalDays;
        response.IncludedKm = includedKm;
        response.TotalPrice = rental.BasePrice + rental.LateFee + rental.ExtraKmCharge;
        response.CarBrand = car?.Brand;
        response.CarModel = car?.Model;
        return response;
    }
}
