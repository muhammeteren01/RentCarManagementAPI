using Core.Repositories;
using Core.UnitOfWork;
using Repository.Data;

namespace Repository.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(
        AppDbContext context,
        IUserRepository users,
        ICarRepository cars,
        IRentalRepository rentals,
        IDamageReportRepository damageReports)
    {
        _context = context;
        Users = users;
        Cars = cars;
        Rentals = rentals;
        DamageReports = damageReports;
    }

    public IUserRepository Users { get; }
    public ICarRepository Cars { get; }
    public IRentalRepository Rentals { get; }
    public IDamageReportRepository DamageReports { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        // DbContext lifetime is managed by DI
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
