using OrbitView.Api.Models;

namespace OrbitView.Api.Repositories;

public interface ITleRepository
{
    Task<List<Satellite>> GetAllActiveSatellitesAsync();
    Task SetAllNotCurrentAsync(int satelliteId);
    Task AddTleRecordAsync(TleRecord record);
    Task AddFetchLogAsync(TleFetchLog log);
    Task SaveChangesAsync();
}