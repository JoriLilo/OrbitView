namespace OrbitView.Api.DTOs;
public class TleRecordDto
{
    public string Line1 { get; set; } = string.Empty;
    public string Line2 { get; set; } = string.Empty;
    public DateTime Epoch { get; set; }
    public decimal Inclination { get; set; }
    public DateTime FetchedAt { get; set; }
}