namespace OrbitView.Api.Models;

public class Satellite
{
    
    public int Id{get; set;}
    public int NoradId{get; set;}
    public string Name{get; set;}=string.Empty;
    public int CategoryId{get; set;}
    public string CountryOfOrigin{get; set;}=string.Empty;
    public DateTime LaunchDate{get; set;}
    public string MissionDescription{get; set;}=string.Empty;
    public bool IsActive{get; set;}
    public DateTime CreatedAt{get; set;}
    public DateTime UpdatedAt{get; set;}





}