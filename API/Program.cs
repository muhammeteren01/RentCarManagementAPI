using API;
using Core.Validations.Users;
using FluentValidation;
using Repository;
using Serilog;
using Service;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

    builder.Services.AddControllers();
    builder.Services.AddSwaggerDocumentation();
    builder.Services.AddRepository(builder.Configuration);
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();

    var app = builder.Build();

    // Outer → Inner: request log must see final status after exception handling
    app.UseCorrelationId();
    app.UseRequestLogging();
    app.UseExceptionMiddleware();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Rent Car Management API v1");
            options.RoutePrefix = "swagger";
        });
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("Application starting. Environment={Environment}", app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
