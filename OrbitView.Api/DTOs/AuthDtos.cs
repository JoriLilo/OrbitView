namespace OrbitView.Api.DTOs;

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class UserProfileDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? LocationName { get; set; }
    public decimal? LocationLat { get; set; }
    public decimal? LocationLon { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateProfileDto
{
    public decimal? LocationLat { get; set; }
    public decimal? LocationLon { get; set; }
    public string? LocationName { get; set; }
}