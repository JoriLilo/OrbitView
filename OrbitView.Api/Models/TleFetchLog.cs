using System.Runtime.CompilerServices;

namespace OrbitView.Api.Models;

public class TleFetchLog
{
    public int Id{get; set;}
    public DateTime FetchedAt{get; set;} = DateTime.UtcNow;
    public string Source{get; set;}= string.Empty;
    public int SatellitesUpdated{get; set;}
    public bool Success{get; set;}
    public string? ErrorMessage{get; set;}
    public int? TriggeredByUserId{get; set;}
    public User? TriggeredBy { get; set; }

}