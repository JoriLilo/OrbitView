using OrbitView.Api.DTOs;
using OrbitView.Api.Models;
using OrbitView.Api.Repositories;

namespace OrbitView.Api.Services;

public class FavouriteService : IFavouriteService
{
    private readonly IFavouriteRepository _repo;

    public FavouriteService(IFavouriteRepository repo)
    {
        _repo = repo;
    }

    public async Task<FavouriteListDto> GetUserFavouritesAsync(int userId)
    {
        var favourites = await _repo.GetByUserIdAsync(userId);

        return new FavouriteListDto
        {
            Total = favourites.Count,
            Data = favourites.Select(f => new FavouriteDto
            {
                Id = f.Id,
                SavedAt = f.SavedAt,
                Notes = f.Notes,
                Satellite = new SatelliteDto
                {
                    Id = f.Satellite.Id,
                    NoradId = f.Satellite.NoradId,
                    Name = f.Satellite.Name,
                    IsActive = f.Satellite.IsActive,
                    Category = f.Satellite.Category == null ? null : new SatelliteCategoryDto
                    {
                        Slug = f.Satellite.Category.Slug,
                        DisplayName = f.Satellite.Category.DisplayName,
                        ColourHex = f.Satellite.Category.ColourHex
                    },
                    CurrentTle = f.Satellite.TleRecords.FirstOrDefault() == null
                        ? null
                        : new TleRecordDto
                        {
                            Line1 = f.Satellite.TleRecords.First().Line1,
                            Line2 = f.Satellite.TleRecords.First().Line2,
                            Epoch = f.Satellite.TleRecords.First().Epoch,
                            Inclination = f.Satellite.TleRecords.First().Inclination,
                            FetchedAt = f.Satellite.TleRecords.First().FetchedAt
                        }
                }
            }).ToList()
        };
    }

    public async Task<FavouriteDto> AddFavouriteAsync(int userId, AddFavouriteDto dto)
    {
        if (await _repo.ExistsAsync(userId, dto.SatelliteId))
            throw new InvalidOperationException("Satellite already in favourites.");

        var favourite = new Favourite
        {
            UserId = userId,
            SatelliteId = dto.SatelliteId,
            Notes = dto.Notes,
            SavedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(favourite);
        await _repo.SaveChangesAsync();

        // Reload with satellite data
        var saved = await _repo.GetByUserIdAsync(userId);
        var result = saved.First(f => f.SatelliteId == dto.SatelliteId);

        return new FavouriteDto
        {
            Id = result.Id,
            SavedAt = result.SavedAt,
            Notes = result.Notes,
            Satellite = new SatelliteDto
            {
                Id = result.Satellite.Id,
                NoradId = result.Satellite.NoradId,
                Name = result.Satellite.Name,
                IsActive = result.Satellite.IsActive,
                Category = result.Satellite.Category == null ? null : new SatelliteCategoryDto
                {
                    Slug = result.Satellite.Category.Slug,
                    DisplayName = result.Satellite.Category.DisplayName,
                    ColourHex = result.Satellite.Category.ColourHex
                }
            }
        };
    }

    public async Task DeleteFavouriteAsync(int userId, int favouriteId)
    {
        var favourite = await _repo.GetByIdAsync(favouriteId);

        if (favourite == null)
            throw new KeyNotFoundException("Favourite not found.");

        if (favourite.UserId != userId)
            throw new UnauthorizedAccessException("You can only remove your own favourites.");

        await _repo.DeleteAsync(favourite);
        await _repo.SaveChangesAsync();
    }
}