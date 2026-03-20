using OrbitView.Api.DTOs;

namespace OrbitView.Api.Services;

public interface IFavouriteService
{
    Task<FavouriteListDto> GetUserFavouritesAsync(int userId);
    Task<FavouriteDto> AddFavouriteAsync(int userId, AddFavouriteDto dto);
    Task DeleteFavouriteAsync(int userId, int favouriteId);
}