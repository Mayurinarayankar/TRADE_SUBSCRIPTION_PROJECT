using TradeSubscriptionAPI.DTOs.Request;
using TradeSubscriptionAPI.DTOs.Response;
using TradeSubscriptionAPI.Models;
using TradeSubscriptionAPI.Repositories.Interfaces;
using TradeSubscriptionAPI.Services.Interfaces;

namespace TradeSubscriptionAPI.Services;

public class CompanySubscriptionService : ICompanySubscriptionService
{
    private readonly ICompanySubscriptionRepository _repo;
    private readonly ICompanyRepository _companyRepo;
    private readonly ISubscriptionPlanRepository _planRepo;

    public CompanySubscriptionService(
        ICompanySubscriptionRepository repo,
        ICompanyRepository companyRepo,
        ISubscriptionPlanRepository planRepo)
    {
        _repo = repo;
        _companyRepo = companyRepo;
        _planRepo = planRepo;
    }

    public async Task<PagedResponse<CompanySubscriptionResponse>> GetPagedAsync(
        int page, int pageSize, int? companyId, SubscriptionStatus? status)
    {
        var (items, total) = await _repo.GetPagedAsync(page, pageSize, companyId, status);
        return new PagedResponse<CompanySubscriptionResponse>
        {
            Items = items.Select(Map).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CompanySubscriptionResponse?> GetByIdAsync(int id)
    {
        var sub = await _repo.GetWithDetailsAsync(id);
        return sub == null ? null : Map(sub);
    }

    public async Task<CompanySubscriptionResponse> CreateAsync(CreateCompanySubscriptionRequest request)
    {
        if (!await _companyRepo.ExistsAsync(request.CompanyId))
            throw new KeyNotFoundException($"Company {request.CompanyId} not found.");

        if (!await _planRepo.ExistsAsync(request.SubscriptionPlanId))
            throw new KeyNotFoundException($"Subscription plan {request.SubscriptionPlanId} not found.");

        var sub = new CompanySubscription
        {
            CompanyId = request.CompanyId,
            SubscriptionPlanId = request.SubscriptionPlanId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            BillingCycle = request.BillingCycle,
            AmountPaid = request.AmountPaid,
            Currency = request.Currency,
            AutoRenew = request.AutoRenew,
            Notes = request.Notes,
            Status = SubscriptionStatus.Active
        };

        await _repo.AddAsync(sub);
        var created = await _repo.GetWithDetailsAsync(sub.Id);
        return Map(created!);
    }

    public async Task<CompanySubscriptionResponse?> UpdateAsync(int id, UpdateCompanySubscriptionRequest request)
    {
        var sub = await _repo.GetWithDetailsAsync(id);
        if (sub == null) return null;

        if (!await _companyRepo.ExistsAsync(request.CompanyId))
            throw new KeyNotFoundException($"Company {request.CompanyId} not found.");

        if (!await _planRepo.ExistsAsync(request.SubscriptionPlanId))
            throw new KeyNotFoundException($"Subscription plan {request.SubscriptionPlanId} not found.");

        sub.CompanyId = request.CompanyId;
        sub.SubscriptionPlanId = request.SubscriptionPlanId;
        sub.StartDate = request.StartDate;
        sub.EndDate = request.EndDate;
        sub.BillingCycle = request.BillingCycle;
        sub.AmountPaid = request.AmountPaid;
        sub.Currency = request.Currency;
        sub.AutoRenew = request.AutoRenew;
        sub.Notes = request.Notes;
        sub.Status = request.Status;

        await _repo.UpdateAsync(sub);
        var updated = await _repo.GetWithDetailsAsync(sub.Id);
        return Map(updated!);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var sub = await _repo.GetByIdAsync(id);
        if (sub == null) return false;
        await _repo.DeleteAsync(sub);
        return true;
    }

    public async Task<IEnumerable<CompanySubscriptionResponse>> GetByCompanyAsync(int companyId)
        => (await _repo.GetByCompanyAsync(companyId)).Select(Map);

    private static CompanySubscriptionResponse Map(CompanySubscription cs) => new()
    {
        Id = cs.Id,
        CompanyId = cs.CompanyId,
        CompanyName = cs.Company?.Name ?? string.Empty,
        SubscriptionPlanId = cs.SubscriptionPlanId,
        PlanName = cs.SubscriptionPlan?.Name ?? string.Empty,
        PlanTier = cs.SubscriptionPlan?.Tier.ToString() ?? string.Empty,
        StartDate = cs.StartDate,
        EndDate = cs.EndDate,
        BillingCycle = cs.BillingCycle.ToString(),
        AmountPaid = cs.AmountPaid,
        Currency = cs.Currency,
        Status = cs.Status.ToString(),
        AutoRenew = cs.AutoRenew,
        Notes = cs.Notes,
        CreatedAt = cs.CreatedAt
    };
}
