using Core.Entities;
using Core.Repositories;
using Core.Services;
using Core.UnitOfWork;
using Service.Services.Common;

namespace Service.Services.DamageReports;

public class DamageReportService : GenericService<DamageReport>, IDamageReportService
{
    public DamageReportService(IDamageReportRepository repository, IUnitOfWork unitOfWork)
        : base(repository, unitOfWork)
    {
    }
}
