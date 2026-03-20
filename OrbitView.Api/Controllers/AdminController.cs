using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrbitView.Api.Data;
using OrbitView.Api.Services;

namespace OrbitView.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITleService _tleService;

    public AdminController(AppDbContext context, ITleService tleService)
    {
        _context = context;
        _tleService = tleService;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var totalSatellites = await _context.Satellites.CountAsync();
        var activeSatellites = await _context.Satellites.CountAsync(s => s.IsActive);
        var totalUsers = await _context.Users.CountAsync();

        var lastFetch = await _context.TleFetchLogs
            .OrderByDescending(l => l.FetchedAt)
            .FirstOrDefaultAsync();

        return Ok(new
        {
            totalSatellites,
            activeSatellites,
            totalUsers,
            lastFetch = lastFetch == null ? null : new
            {
                lastFetch.FetchedAt,
                lastFetch.Success,
                lastFetch.SatellitesUpdated,
                lastFetch.Source
            },
            nextScheduledFetch = lastFetch == null
                ? DateTime.UtcNow.AddHours(1)
                : lastFetch.FetchedAt.AddHours(1)
        });
    }

    [HttpGet("tle-logs")]
    public async Task<IActionResult> GetTleLogs()
    {
        var logs = await _context.TleFetchLogs
            .Include(l => l.TriggeredBy)
            .OrderByDescending(l => l.FetchedAt)
            .Take(20)
            .Select(l => new
            {
                l.Id,
                l.FetchedAt,
                l.Source,
                l.SatellitesUpdated,
                l.Success,
                l.ErrorMessage,
                triggeredBy = l.TriggeredBy == null ? null : new
                {
                    l.TriggeredBy.Id,
                    l.TriggeredBy.Username
                }
            })
            .ToListAsync();

        return Ok(new { data = logs });
    }

    [HttpPost("tle-refresh")]
    public async Task<IActionResult> TriggerRefresh()
    {
        var result = await _tleService.FetchAndStoreAsync();
        return Ok(result);
    }

    [HttpPut("satellites/{id}")]
    public async Task<IActionResult> UpdateSatellite(int id,
        [FromBody] UpdateSatelliteDto dto)
    {
        var satellite = await _context.Satellites.FindAsync(id);
        if (satellite == null) return NotFound(new { error = "Satellite not found" });

        if (dto.MissionDescription != null)
            satellite.MissionDescription = dto.MissionDescription;
        if (dto.IsActive.HasValue)
            satellite.IsActive = dto.IsActive.Value;
        if (dto.CategoryId.HasValue)
            satellite.SatelliteCategoryId = dto.CategoryId.Value;

        satellite.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            satellite.Id,
            satellite.Name,
            satellite.MissionDescription,
            satellite.IsActive,
            satellite.UpdatedAt
        });
    }
}

public class UpdateSatelliteDto
{
    public string? MissionDescription { get; set; }
    public bool? IsActive { get; set; }
    public int? CategoryId { get; set; }
}