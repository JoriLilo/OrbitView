using Microsoft.EntityFrameworkCore;
using OrbitView.Api.Data;
using OrbitView.Api.Models;

namespace OrbitView.Api.Repositories;

public class FavouriteRepository : IFavouriteRepository
{
    private readonly AppDbContext _context;

    public FavouriteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Favourite>> GetByUserIdAsync(int userId)
    {
        return await _context.Favourites
            .Include(f => f.Satellite)
                .ThenInclude(s => s.Category)
            .Include(f => f.Satellite)
                .ThenInclude(s => s.TleRecords.Where(t => t.IsCurrent))
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.SavedAt)
            .ToListAsync();
    }

    public async Task<Favourite?> GetByIdAsync(int id)
    {
        return await _context.Favourites
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<bool> ExistsAsync(int userId, int satelliteId)
    {
        return await _context.Favourites
            .AnyAsync(f => f.UserId == userId && f.SatelliteId == satelliteId);
    }

    public async Task AddAsync(Favourite favourite)
    {
        await _context.Favourites.AddAsync(favourite);
    }

    public async Task DeleteAsync(Favourite favourite)
    {
        _context.Favourites.Remove(favourite);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}