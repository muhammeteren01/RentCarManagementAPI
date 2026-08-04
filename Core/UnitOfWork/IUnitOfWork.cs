using Core.Repositories;

namespace Core.UnitOfWork;

public interface IUnitOfWork : IAsyncDisposable
{
    IUserRepository Users { get; }
    ICarRepository Cars { get; }
    IRentalRepository Rentals { get; }
    IDamageReportRepository DamageReports { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
