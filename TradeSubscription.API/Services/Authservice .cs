using TradeSubscriptionAPI.DTOs.Request;
using TradeSubscriptionAPI.DTOs.Response;
using TradeSubscriptionAPI.Helpers;
using TradeSubscriptionAPI.Models;
using TradeSubscriptionAPI.Repositories.Interfaces;
using TradeSubscriptionAPI.Services.Interfaces;

namespace TradeSubscriptionAPI.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtHelper _jwt;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository userRepo, IJwtHelper jwt, IConfiguration config)
    {
        _userRepo = userRepo;
        _jwt = jwt;
        _config = config;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (!await _userRepo.IsEmailUniqueAsync(request.Email))
            throw new InvalidOperationException("Email already registered.");

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            role = UserRole.User;

        var user = new AppUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
            IsActive = true
        };

        await _userRepo.AddAsync(user);
        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var users = await _userRepo.FindAsync(u => u.RefreshToken == refreshToken);
        var user = users.FirstOrDefault()
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expired.");

        return await BuildAuthResponseAsync(user);
    }

    public async Task RevokeTokenAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userRepo.UpdateAsync(user);
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(AppUser user)
    {
        var accessToken = _jwt.GenerateAccessToken(user);
        var refreshToken = _jwt.GenerateRefreshToken();
        var expiryMinutes = Convert.ToDouble(_config["Jwt:ExpiryMinutes"] ?? "60");
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepo.UpdateAsync(user);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            }
        };
    }
}