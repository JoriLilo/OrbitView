using OrbitView.Api.DTOs;

namespace OrbitView.Api.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<UserProfileDto?> GetProfileAsync(int userId);
    Task<UserProfileDto?> UpdateProfileAsync(int userId, UpdateProfileDto dto);
}