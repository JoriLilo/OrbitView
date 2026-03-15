using OrbitView.Api.DTOs;

namespace OrbitView.Api.Services;

public interface ISatelliteService
{
    Task<SatelliteListDto> GetAllAsync(
        string? category, string? search, bool? isActive, int page, int pageSize);
    Task<SatelliteDetailDto?> GetByIdAsync(int id);
}