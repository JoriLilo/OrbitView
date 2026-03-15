using Microsoft.EntityFrameworkCore;
using OrbitView.Api.Data;
using OrbitView.Api.Models;

namespace OrbitView.Api.Repositories;

public class TleRepository : ITleRepository
{
    private readonly AppDbContext _context;

    public TleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Satellite>> GetAllActiveSatellitesAsync()
    {
        return await _context.Satellites
            .Where(s => s.IsActive)
            .ToListAsync();
    }

    public async Task SetAllNotCurrentAsync(int satelliteId)
    {
        await _context.TleRecords
            .Where(t => t.SatelliteId == satelliteId && t.IsCurrent)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsCurrent, false));
    }

    public async Task AddTleRecordAsync(TleRecord record)
    {
        await _context.TleRecords.AddAsync(record);
    }

    public async Task AddFetchLogAsync(TleFetchLog log)
    {
        await _context.TleFetchLogs.AddAsync(log);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}