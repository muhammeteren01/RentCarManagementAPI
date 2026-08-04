using Core.Entities;
using Core.Repositories;
using Core.Services;
using Core.UnitOfWork;

namespace Service.Services;

public class DamageReportService : GenericService<DamageReport>, IDamageReportService
{
    public DamageReportService(IDamageReportRepository repository, IUnitOfWork unitOfWork)
        : base(repository, unitOfWork)
    {
    }
}
