using Core.Entities;
using Core.Repositories;
using Core.Services;
using Core.UnitOfWork;

namespace Service.Services;

public class UserService : GenericService<User>, IUserService
{
    public UserService(IUserRepository repository, IUnitOfWork unitOfWork)
        : base(repository, unitOfWork)
    {
    }
}
