using Core.Validations.Users;
using FluentValidation;
using Repository;
using Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddRepository(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
