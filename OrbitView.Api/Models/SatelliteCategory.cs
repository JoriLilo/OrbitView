namespace OrbitView.Api.Models;

public class SatelliteCategory
{
    public int Id{get; set;}
    public string Slug{get; set;}= string.Empty;
    public string DisplayName{get; set;}= string.Empty;
    public string ColourHex { get; set; } = string.Empty;
     public ICollection<Satellite> Satellites { get; set; } = new List<Satellite>();


}