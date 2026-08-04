using Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Service.Mapping;
using Service.Services;

namespace Service;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICarService, CarService>();
        services.AddScoped<IRentalService, RentalService>();
        services.AddScoped<IDamageReportService, DamageReportService>();

        services.AddAutoMapper(cfg => { }, typeof(MappingProfile));

        return services;
    }
}
