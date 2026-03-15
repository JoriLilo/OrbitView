namespace OrbitView.Api.DTOs;

public class SatelliteDetailDto : SatelliteDto
{
    public string? CountryOfOrigin { get; set; }
    public DateTime? LaunchDate { get; set; }
    public string? MissionDescription { get; set; }
    public bool IsFavouritedByCurrentUser { get; set; }
}