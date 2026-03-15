namespace OrbitView.Api.Models;

public class TleRecord
{
    public int Id { get; set; }
    public int SatelliteId { get; set; }
    public Satellite Satellite { get; set; } = null!;
    public string Line1 { get; set; } = string.Empty;
    public string Line2 { get; set; } = string.Empty;
    public DateTime Epoch { get; set; }
    public decimal Inclination { get; set; }
    public decimal Eccentricity { get; set; }
    public decimal MeanMotion { get; set; }
    public bool IsCurrent { get; set; } = true;
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
}