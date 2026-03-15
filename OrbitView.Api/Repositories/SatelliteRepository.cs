using Microsoft.EntityFrameworkCore;
using OrbitView.Api.Data;
using OrbitView.Api.Models;

namespace OrbitView.Api.Repositories;

public class SatelliteRepository : ISatelliteRepository
{
    private readonly AppDbContext _context;

    public SatelliteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Satellite> satellites, int total)> GetAllAsync(
        string? category, string? search, bool? isActive, int page, int pageSize)
    {
        var query = _context.Satellites
            .Include(s => s.Category)
            .Include(s => s.TleRecords.Where(t => t.IsCurrent))
            .AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(s => s.Category.Slug == category);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(s => s.Name.Contains(search) ||
                                     s.NoradId.ToString().Contains(search));

        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);

        var total = await query.CountAsync();
        var satellites = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (satellites, total);
    }

    public async Task<Satellite?> GetByIdAsync(int id)
    {
        return await _context.Satellites
            .Include(s => s.Category)
            .Include(s => s.TleRecords.Where(t => t.IsCurrent))
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Satellite?> GetByNoradIdAsync(int noradId)
    {
        return await _context.Satellites
            .FirstOrDefaultAsync(s => s.NoradId == noradId);
    }
}