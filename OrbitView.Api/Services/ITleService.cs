using OrbitView.Api.DTOs;

namespace OrbitView.Api.Services;

public interface ITleService
{
    Task<TleFetchResultDto> FetchAndStoreAsync(int? triggeredByUserId = null);
}