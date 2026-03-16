using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OrbitView.Api.Data;
using OrbitView.Api.DTOs;
using OrbitView.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OrbitView.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            throw new InvalidOperationException("Email already registered.");

        // Check if username already exists
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            throw new InvalidOperationException("Username already taken.");

        // Validate password
        if (dto.Password.Length < 8 || !dto.Password.Any(char.IsDigit))
            throw new InvalidOperationException(
                "Password must be at least 8 characters and contain a number.");

        // Get the default User role
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User")
            ?? throw new InvalidOperationException("User role not found. Run seed data.");

        var user = new User
        {
            Email = dto.Email.ToLower().Trim(),
            Username = dto.Username.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = role.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return BuildAuthResponse(user, role.Name);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower().Trim());

        // Never say which field is wrong — security best practice
        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return BuildAuthResponse(user, user.Role.Name);
    }

    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Role = user.Role.Name,
            LocationName = user.LocationName,
            LocationLat = user.LocationLat,
            LocationLon = user.LocationLon,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserProfileDto?> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        if (dto.LocationLat.HasValue) user.LocationLat = dto.LocationLat;
        if (dto.LocationLon.HasValue) user.LocationLon = dto.LocationLon;
        if (dto.LocationName != null) user.LocationName = dto.LocationName;

        await _context.SaveChangesAsync();

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Role = user.Role.Name,
            LocationName = user.LocationName,
            LocationLat = user.LocationLat,
            LocationLon = user.LocationLon,
            CreatedAt = user.CreatedAt
        };
    }

    private AuthResponseDto BuildAuthResponse(User user, string roleName)
    {
        var token = GenerateJwtToken(user, roleName);
        var expiryHours = _config.GetValue<int>("Jwt:ExpiryHours");

        return new AuthResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Role = roleName,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(expiryHours)
        };
    }

    private string GenerateJwtToken(User user, string roleName)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, roleName)
        };

        var expiryHours = _config.GetValue<int>("Jwt:ExpiryHours");

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiryHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}