using Core.Repositories;
using Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository.Data;
using Repository.Repositories;
using Repository.UnitOfWork;

namespace Repository;

public static class DependencyInjection
{
    public static IServiceCollection AddRepository(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICarRepository, CarRepository>();
        services.AddScoped<IRentalRepository, RentalRepository>();
        services.AddScoped<IDamageReportRepository, DamageReportRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        return services;
    }
}
