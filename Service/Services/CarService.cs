using AutoMapper;
using Core.DTOs.Cars;
using Core.Entities;
using Core.Enums;
using Core.Exceptions;
using Core.Repositories;
using Core.Services;
using Core.UnitOfWork;
using FluentValidation;
using ValidationException = Core.Validations.ValidationException;

namespace Service.Services;

public class CarService : GenericService<Car>, ICarService
{
    private readonly ICarRepository _carRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCarRequest> _createValidator;
    private readonly IValidator<UpdateCarRequest> _updateValidator;

    public CarService(
        ICarRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateCarRequest> createValidator,
        IValidator<UpdateCarRequest> updateValidator)
        : base(repository, unitOfWork)
    {
        _carRepository = repository;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IEnumerable<CarResponse>> GetCarsAsync(UserRole role)
    {
        IEnumerable<Car> cars = role == UserRole.Admin
            ? await _carRepository.GetAllAsync()
            : await _carRepository.FindAsync(c => c.Status == CarStatus.Available);

        return _mapper.Map<IEnumerable<CarResponse>>(cars);
    }

    public async Task<CarResponse> GetCarByIdAsync(Guid id)
    {
        var car = await _carRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Car with id '{id}' was not found.");

        return _mapper.Map<CarResponse>(car);
    }

    public async Task<CarResponse> CreateAsync(CreateCarRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var car = _mapper.Map<Car>(request);
        car.Id = Guid.NewGuid();
        car.CreatedAt = DateTime.UtcNow;

        await _carRepository.AddAsync(car);
        await UnitOfWork.SaveChangesAsync();

        return _mapper.Map<CarResponse>(car);
    }

    public async Task<CarResponse> UpdateAsync(Guid id, UpdateCarRequest request)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var car = await _carRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Car with id '{id}' was not found.");

        _mapper.Map(request, car);
        _carRepository.Update(car);
        await UnitOfWork.SaveChangesAsync();

        return _mapper.Map<CarResponse>(car);
    }

    public async Task DeleteAsync(Guid id)
    {
        var car = await _carRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Car with id '{id}' was not found.");

        if (car.Status == CarStatus.Rented)
        {
            throw new ConflictException("Cannot delete a car that is currently rented.");
        }

        _carRepository.Remove(car);
        await UnitOfWork.SaveChangesAsync();
    }
}
