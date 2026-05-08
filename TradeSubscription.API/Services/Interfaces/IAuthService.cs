using TradeSubscriptionAPI.DTOs.Request;
using TradeSubscriptionAPI.DTOs.Response;
using TradeSubscriptionAPI.Models;

namespace TradeSubscriptionAPI.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task RevokeTokenAsync(int userId);
}

public interface ICompanySubscriptionService
{
    Task<PagedResponse<CompanySubscriptionResponse>> GetPagedAsync(int page, int pageSize, int? companyId, SubscriptionStatus? status);
    Task<CompanySubscriptionResponse?> GetByIdAsync(int id);
    Task<CompanySubscriptionResponse> CreateAsync(CreateCompanySubscriptionRequest request);
    Task<CompanySubscriptionResponse?> UpdateAsync(int id, UpdateCompanySubscriptionRequest request);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<CompanySubscriptionResponse>> GetByCompanyAsync(int companyId);
}