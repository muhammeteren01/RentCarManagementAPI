using System.Security.Claims;
using Core.DTOs.DamageReports;
using Core.Enums;
using Core.Exceptions;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DamageReportsController : ControllerBase
{
    private readonly IDamageReportService _damageReportService;

    public DamageReportsController(IDamageReportService damageReportService)
    {
        _damageReportService = damageReportService;
    }

    [HttpPost]
    public async Task<ActionResult<DamageReportResponse>> Create([FromBody] CreateDamageReportRequest request)
    {
        var (userId, role) = GetCurrentUser();
        var report = await _damageReportService.CreateAsync(userId, role, request);
        return Created($"/api/DamageReports/{report.Id}", report);
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
