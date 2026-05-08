using TradeSubscriptionAPI.DTOs.Request;
using TradeSubscriptionAPI.DTOs.Response;

namespace TradeSubscriptionAPI.Services.Interfaces;

public interface ITradeService
{
    Task<PagedResponse<TradeResponse>> GetPagedAsync(TradeFilterRequest filter);
    Task<TradeResponse?> GetByIdAsync(int id);
    Task<TradeResponse> CreateAsync(CreateTradeRequest request);
    Task<TradeResponse?> UpdateAsync(int id, UpdateTradeRequest request);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<TradeResponse>> GetByCompanyAsync(int companyId);
}