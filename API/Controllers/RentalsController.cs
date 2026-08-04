using System.Security.Claims;
using Core.DTOs.Rentals;
using Core.Enums;
using Core.Exceptions;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RentalsController : ControllerBase
{
    private readonly IRentalService _rentalService;

    public RentalsController(IRentalService rentalService)
    {
        _rentalService = rentalService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RentalResponse>>> GetHistory()
    {
        var (userId, role) = GetCurrentUser();
        var rentals = await _rentalService.GetHistoryAsync(userId, role);
        return Ok(rentals);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RentalResponse>> GetById(Guid id)
    {
        var (userId, role) = GetCurrentUser();
        var rental = await _rentalService.GetByIdAsync(id, userId, role);
        return Ok(rental);
    }

    [HttpPost]
    public async Task<ActionResult<RentalResponse>> Create([FromBody] CreateRentalRequest request)
    {
        var (userId, _) = GetCurrentUser();
        var rental = await _rentalService.CreateAsync(userId, request);
        return CreatedAtAction(nameof(GetById), new { id = rental.Id }, rental);
    }

    [HttpPost("{id:guid}/extend")]
    public async Task<ActionResult<RentalResponse>> Extend(Guid id, [FromBody] ExtendRentalRequest request)
    {
        var (userId, role) = GetCurrentUser();
        var rental = await _rentalService.ExtendAsync(id, userId, role, request);
        return Ok(rental);
    }

    [HttpPost("{id:guid}/return")]
    public async Task<ActionResult<RentalResponse>> Return(Guid id, [FromBody] ReturnRentalRequest request)
    {
        var (userId, role) = GetCurrentUser();
        var rental = await _rentalService.ReturnAsync(id, userId, role, request);
        return Ok(rental);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<RentalResponse>> Cancel(Guid id)
    {
        var (userId, role) = GetCurrentUser();
        var rental = await _rentalService.CancelAsync(id, userId, role);
        return Ok(rental);
    }

    private (Guid UserId, UserRole Role) GetCurrentUser()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new UnauthorizedException("Invalid user token.");
        }

        var role = User.IsInRole(nameof(UserRole.Admin))
            ? UserRole.Admin
            : UserRole.Customer;

        return (userId, role);
    }
}
