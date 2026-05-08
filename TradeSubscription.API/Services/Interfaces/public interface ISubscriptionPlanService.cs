using TradeSubscriptionAPI.DTOs.Request;
using TradeSubscriptionAPI.DTOs.Response;

namespace TradeSubscriptionAPI.Services.Interfaces;
public interface ISubscriptionPlanService
{
    Task<IEnumerable<SubscriptionPlanResponse>> GetAllAsync();
    Task<IEnumerable<SubscriptionPlanResponse>> GetActiveAsync();
    Task<SubscriptionPlanResponse?> GetByIdAsync(int id);
    Task<SubscriptionPlanResponse> CreateAsync(CreateSubscriptionPlanRequest request);
    Task<SubscriptionPlanResponse?> UpdateAsync(int id, UpdateSubscriptionPlanRequest request);
    Task<bool> DeleteAsync(int id);
}