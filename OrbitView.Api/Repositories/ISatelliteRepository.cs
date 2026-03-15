using OrbitView.Api.Models;

namespace OrbitView.Api.Repositories;

public interface ISatelliteRepository
{
    Task<(List<Satellite> satellites, int total)> GetAllAsync(
        string? category, string? search, bool? isActive, int page, int pageSize);
    Task<Satellite?> GetByIdAsync(int id);
    Task<Satellite?> GetByNoradIdAsync(int noradId);
}