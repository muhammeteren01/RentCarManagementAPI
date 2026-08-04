using Core.DTOs.DamageReports;
using Core.Entities;
using Core.Enums;

namespace Core.Services;

public interface IDamageReportService : IGenericService<DamageReport>
{
    Task<DamageReportResponse> CreateAsync(Guid requesterId, UserRole role, CreateDamageReportRequest request);
    Task<DamageReportResponse> CollectPaymentAsync(Guid damageReportId);
}
