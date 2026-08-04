using Core.DTOs.Cars;
using Core.Enums;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarsController : ControllerBase
{
    private readonly ICarService _carService;

    public CarsController(ICarService carService)
    {
        _carService = carService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<CarResponse>>> GetAll()
    {
        var role = User.IsInRole(nameof(UserRole.Admin))
            ? UserRole.Admin
            : UserRole.Customer;

        var cars = await _carService.GetCarsAsync(role);
        return Ok(cars);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<CarResponse>> GetById(Guid id)
    {
        var car = await _carService.GetCarByIdAsync(id);
        return Ok(car);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<CarResponse>> Create([FromBody] CreateCarRequest request)
    {
        var car = await _carService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = car.Id }, car);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<CarResponse>> Update(Guid id, [FromBody] UpdateCarRequest request)
    {
        var car = await _carService.UpdateAsync(id, request);
        return Ok(car);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _carService.DeleteAsync(id);
        return NoContent();
    }
}
