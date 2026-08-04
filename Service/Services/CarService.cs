using Core.Entities;
using Core.Repositories;
using Core.Services;
using Core.UnitOfWork;

namespace Service.Services;

public class CarService : GenericService<Car>, ICarService
{
    public CarService(ICarRepository repository, IUnitOfWork unitOfWork)
        : base(repository, unitOfWork)
    {
    }
}
