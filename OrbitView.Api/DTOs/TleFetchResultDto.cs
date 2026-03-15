namespace OrbitView.Api.DTOs;

public class TleFetchResultDto
{
    public bool Success { get; set; }
    public int SatellitesUpdated { get; set; }
    public DateTime FetchedAt { get; set; }
    public string? ErrorMessage { get; set; }
}