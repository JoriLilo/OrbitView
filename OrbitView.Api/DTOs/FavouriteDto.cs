namespace OrbitView.Api.DTOs;

public class FavouriteDto
{
    public int Id { get; set; }
    public DateTime SavedAt { get; set; }
    public string? Notes { get; set; }
    public SatelliteDto Satellite { get; set; } = null!;
}

public class FavouriteListDto
{
    public int Total { get; set; }
    public List<FavouriteDto> Data { get; set; } = new();
}

public class AddFavouriteDto
{
    public int SatelliteId { get; set; }
    public string? Notes { get; set; }
}