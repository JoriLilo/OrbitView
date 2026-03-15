namespace OrbitView.Api.Models;

 public class Satellite
{
    
    public int Id{get; set;}
    public int NoradId{get; set;}
    public string Name{get; set;}=string.Empty;
    public int CategoryId{get; set;}
    public SatelliteCategory SatelliteCategory { get; set; } =null!;
    public string CountryOfOrigin{get; set;}=string.Empty;
    public DateTime? LaunchDate{get; set;}
    public string? MissionDescription{get; set;}=string.Empty;
    public bool IsActive{get; set;}=true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<TleRecord> TleRecords { get; set; } = new List<TleRecord>();
    public ICollection<Favourite> Favourites { get; set; } = new List<Favourite>();
}





