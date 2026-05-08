using TradeSubscriptionAPI.DTOs.Request;
using TradeSubscriptionAPI.DTOs.Response;

namespace TradeSubscriptionAPI.Services.Interfaces;
public interface IIncotermService
{
    Task<IEnumerable<IncotermResponse>> GetAllAsync();
    Task<IEnumerable<IncotermResponse>> GetActiveAsync();
    Task<IncotermResponse?> GetByIdAsync(int id);
    Task<IncotermResponse> CreateAsync(CreateIncotermRequest request);
    Task<IncotermResponse?> UpdateAsync(int id, UpdateIncotermRequest request);
    Task<bool> DeleteAsync(int id);
}