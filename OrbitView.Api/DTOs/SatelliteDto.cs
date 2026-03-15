namespace OrbitView.Api.DTOs;
public class SatelliteDto
{
    public int Id { get; set; }
    public int NoradId { get; set; }
    public string Name { get; set; } = string.Empty;
    public SatelliteCategoryDto? Category { get; set; }
    public bool IsActive { get; set; }
    public TleRecordDto? CurrentTle { get; set; }
}