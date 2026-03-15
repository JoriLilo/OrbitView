using System.Resources;

namespace OrbitView.Api.Models;

public class Favourite
{
    
    public int Id{get; set;}
    public int UserId{get; set;}
    public User User { get; set; } = null!;
    public int SatelliteId{get; set;}
    public Satellite Satellite { get; set; } = null!;
    public DateTime SavedAt{get; set;} = DateTime.UtcNow;
    public string? Notes{get; set;}= string.Empty;





}