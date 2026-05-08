using TradeSubscriptionAPI.DTOs.Request;
using TradeSubscriptionAPI.DTOs.Response;
using TradeSubscriptionAPI.Models;

namespace TradeSubscriptionAPI.Services.Interfaces;

public interface ICompanyService
{
    Task<PagedResponse<CompanyResponse>> GetPagedAsync(int page, int pageSize, string? search, CompanyType? type);
    Task<CompanyResponse?> GetByIdAsync(int id);
    Task<CompanyResponse> CreateAsync(CreateCompanyRequest request);
    Task<CompanyResponse?> UpdateAsync(int id, UpdateCompanyRequest request);
    Task<bool> DeleteAsync(int id);
}