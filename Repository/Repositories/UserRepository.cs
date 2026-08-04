using Core.Entities;
using Core.Repositories;
using Repository.Data;

namespace Repository.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }
}
