using TradeSubscriptionAPI.DTOs.Request;
using TradeSubscriptionAPI.DTOs.Response;
using TradeSubscriptionAPI.Models;
using TradeSubscriptionAPI.Repositories.Interfaces;
using TradeSubscriptionAPI.Services.Interfaces;

namespace TradeSubscriptionAPI.Services;

public class SubscriptionPlanService : ISubscriptionPlanService
{
    private readonly ISubscriptionPlanRepository _repo;

    public SubscriptionPlanService(ISubscriptionPlanRepository repo) => _repo = repo;

    public async Task<IEnumerable<SubscriptionPlanResponse>> GetAllAsync()
        => (await _repo.GetAllAsync()).Select(Map);

    public async Task<IEnumerable<SubscriptionPlanResponse>> GetActiveAsync()
        => (await _repo.GetActiveAsync()).Select(Map);

    public async Task<SubscriptionPlanResponse?> GetByIdAsync(int id)
    {
        var plan = await _repo.GetByIdAsync(id);
        return plan == null ? null : Map(plan);
    }

    public async Task<SubscriptionPlanResponse> CreateAsync(CreateSubscriptionPlanRequest request)
    {
        if (!await _repo.IsNameUniqueAsync(request.Name))
            throw new InvalidOperationException($"Plan '{request.Name}' already exists.");

        var plan = new SubscriptionPlan
        {
            Name = request.Name,
            Description = request.Description,
            MonthlyPrice = request.MonthlyPrice,
            AnnualPrice = request.AnnualPrice,
            Currency = request.Currency,
            MaxUsers = request.MaxUsers,
            MaxTrades = request.MaxTrades,
            HasApiAccess = request.HasApiAccess,
            HasReporting = request.HasReporting,
            HasAdvancedAnalytics = request.HasAdvancedAnalytics,
            Tier = request.Tier,
            IsActive = request.IsActive
        };

        await _repo.AddAsync(plan);
        return Map(plan);
    }

    public async Task<SubscriptionPlanResponse?> UpdateAsync(int id, UpdateSubscriptionPlanRequest request)
    {
        var plan = await _repo.GetByIdAsync(id);
        if (plan == null) return null;

        if (!await _repo.IsNameUniqueAsync(request.Name, id))
            throw new InvalidOperationException($"Plan '{request.Name}' already exists.");

        plan.Name = request.Name;
        plan.Description = request.Description;
        plan.MonthlyPrice = request.MonthlyPrice;
        plan.AnnualPrice = request.AnnualPrice;
        plan.Currency = request.Currency;
        plan.MaxUsers = request.MaxUsers;
        plan.MaxTrades = request.MaxTrades;
        plan.HasApiAccess = request.HasApiAccess;
        plan.HasReporting = request.HasReporting;
        plan.HasAdvancedAnalytics = request.HasAdvancedAnalytics;
        plan.Tier = request.Tier;
        plan.IsActive = request.IsActive;

        await _repo.UpdateAsync(plan);
        return Map(plan);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var plan = await _repo.GetByIdAsync(id);
        if (plan == null) return false;
        await _repo.DeleteAsync(plan);
        return true;
    }

    private static SubscriptionPlanResponse Map(SubscriptionPlan p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        MonthlyPrice = p.MonthlyPrice,
        AnnualPrice = p.AnnualPrice,
        Currency = p.Currency,
        MaxUsers = p.MaxUsers,
        MaxTrades = p.MaxTrades,
        HasApiAccess = p.HasApiAccess,
        HasReporting = p.HasReporting,
        HasAdvancedAnalytics = p.HasAdvancedAnalytics,
        Tier = p.Tier.ToString(),
        IsActive = p.IsActive
    };
}