using Core.Entities;
using Core.Repositories;
using Repository.Data;

namespace Repository.Repositories;

public class RentalRepository : GenericRepository<Rental>, IRentalRepository
{
    public RentalRepository(AppDbContext context) : base(context)
    {
    }
}
