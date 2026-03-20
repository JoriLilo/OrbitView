using OrbitView.Api.Models;

namespace OrbitView.Api.Repositories;

public interface IFavouriteRepository
{
    Task<List<Favourite>> GetByUserIdAsync(int userId);
    Task<Favourite?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int userId, int satelliteId);
    Task AddAsync(Favourite favourite);
    Task DeleteAsync(Favourite favourite);
    Task SaveChangesAsync();
}