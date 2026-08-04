using Core.Entities;
using Core.Repositories;
using Repository.Data;

namespace Repository.Repositories;

public class DamageReportRepository : GenericRepository<DamageReport>, IDamageReportRepository
{
    public DamageReportRepository(AppDbContext context) : base(context)
    {
    }
}
