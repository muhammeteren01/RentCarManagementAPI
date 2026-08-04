using Core.Entities;
using Core.Repositories;
using Core.Services;
using Core.UnitOfWork;

namespace Service.Services;

public class RentalService : GenericService<Rental>, IRentalService
{
    public RentalService(IRentalRepository repository, IUnitOfWork unitOfWork)
        : base(repository, unitOfWork)
    {
    }
}
