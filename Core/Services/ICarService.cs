using Core.DTOs.Cars;
using Core.Entities;
using Core.Enums;

namespace Core.Services;

public interface ICarService : IGenericService<Car>
{
    Task<IEnumerable<CarResponse>> GetCarsAsync(UserRole role);
    Task<CarResponse> GetCarByIdAsync(Guid id);
    Task<CarResponse> CreateAsync(CreateCarRequest request);
    Task<CarResponse> UpdateAsync(Guid id, UpdateCarRequest request);
    Task DeleteAsync(Guid id);
}
