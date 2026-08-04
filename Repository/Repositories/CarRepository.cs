using Core.Entities;
using Core.Repositories;
using Repository.Data;

namespace Repository.Repositories;

public class CarRepository : GenericRepository<Car>, ICarRepository
{
    public CarRepository(AppDbContext context) : base(context)
    {
    }
}
