using Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service.Auth;
using Service.Cars;
using Service.DamageReports;
using Service.Mapping;
using Service.Rentals;
using Service.Users;

namespace Service;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICarService, CarService>();
        services.AddScoped<IRentalService, RentalService>();
        services.AddScoped<IDamageReportService, DamageReportService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IPricingService, PricingService>();

        services.AddAutoMapper(cfg => { }, typeof(MappingProfile));

        return services;
    }
}