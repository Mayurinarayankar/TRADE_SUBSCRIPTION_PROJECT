using Microsoft.EntityFrameworkCore;
using TradeSubscriptionAPI.Data;
using TradeSubscriptionAPI.DTOs.Request;
using TradeSubscriptionAPI.Models;
using TradeSubscriptionAPI.Repositories.Interfaces;

namespace TradeSubscriptionAPI.Repositories;

// ── Company ───────────────────────────────────────────────────────────────
public class CompanyRepository : GenericRepository<Company>, ICompanyRepository
{
    public CompanyRepository(AppDbContext context) : base(context) { }

    public async Task<Company?> GetByNameAsync(string name)
        => await _dbSet.FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());

    public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
        => !await _dbSet.AnyAsync(c => c.Name.ToLower() == name.ToLower() && c.Id != excludeId);

    public async Task<(IEnumerable<Company> Items, int Total)> GetPagedAsync(
        int page, int pageSize, string? search = null, CompanyType? type = null)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.Contains(search) ||
                                     (c.Email != null && c.Email.Contains(search)) ||
                                     (c.Country != null && c.Country.Contains(search)));

        if (type.HasValue)
            query = query.Where(c => c.Type == type.Value);

        var total = await query.CountAsync();
        var items = await query.OrderBy(c => c.Name)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();
        return (items, total);
    }
}

// ── Incoterm ──────────────────────────────────────────────────────────────
public class IncotermRepository : GenericRepository<Incoterm>, IIncotermRepository
{
    public IncotermRepository(AppDbContext context) : base(context) { }

    public async Task<Incoterm?> GetByCodeAsync(string code)
        => await _dbSet.FirstOrDefaultAsync(i => i.Code.ToUpper() == code.ToUpper());

    public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
        => !await _dbSet.AnyAsync(i => i.Code.ToUpper() == code.ToUpper() && i.Id != excludeId);

    public async Task<IEnumerable<Incoterm>> GetActiveAsync()
        => await _dbSet.Where(i => i.IsActive).OrderBy(i => i.Code).ToListAsync();
}

// ── Trade ─────────────────────────────────────────────────────────────────
public class TradeRepository : GenericRepository<Trade>, ITradeRepository
{
    public TradeRepository(AppDbContext context) : base(context) { }

    public async Task<Trade?> GetWithDetailsAsync(int id)
        => await _dbSet
            .Include(t => t.Company)
            .Include(t => t.Incoterm)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<bool> IsTradeNumberUniqueAsync(string tradeNumber, int? excludeId = null)
        => !await _dbSet.AnyAsync(t => t.TradeNumber == tradeNumber && t.Id != excludeId);

    public async Task<(IEnumerable<Trade> Items, int Total)> GetPagedAsync(TradeFilterRequest filter)
    {
        var query = _dbSet.Include(t => t.Company).Include(t => t.Incoterm).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            query = query.Where(t => t.TradeNumber.Contains(filter.SearchTerm) ||
                                     t.Commodity.Contains(filter.SearchTerm) ||
                                     (t.Company != null && t.Company.Name.Contains(filter.SearchTerm)));

        if (filter.Status.HasValue)
            query = query.Where(t => t.Status == filter.Status.Value);

        if (filter.CompanyId.HasValue)
            query = query.Where(t => t.CompanyId == filter.CompanyId.Value);

        if (filter.IncotermId.HasValue)
            query = query.Where(t => t.IncotermId == filter.IncotermId.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(t => t.TradeDate >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(t => t.TradeDate <= filter.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.Currency))
            query = query.Where(t => t.Currency == filter.Currency);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(t => t.TradeDate)
                               .Skip((filter.Page - 1) * filter.PageSize)
                               .Take(filter.PageSize)
                               .ToListAsync();
        return (items, total);
    }

    public async Task<IEnumerable<Trade>> GetByCompanyAsync(int companyId)
        => await _dbSet
            .Include(t => t.Incoterm)
            .Where(t => t.CompanyId == companyId)
            .OrderByDescending(t => t.TradeDate)
            .ToListAsync();
}

// ── SubscriptionPlan ──────────────────────────────────────────────────────
public class SubscriptionPlanRepository : GenericRepository<SubscriptionPlan>, ISubscriptionPlanRepository
{
    public SubscriptionPlanRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<SubscriptionPlan>> GetActiveAsync()
        => await _dbSet.Where(s => s.IsActive).OrderBy(s => s.MonthlyPrice).ToListAsync();

    public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
        => !await _dbSet.AnyAsync(s => s.Name.ToLower() == name.ToLower() && s.Id != excludeId);
}

// ── CompanySubscription ───────────────────────────────────────────────────
public class CompanySubscriptionRepository : GenericRepository<CompanySubscription>, ICompanySubscriptionRepository
{
    public CompanySubscriptionRepository(AppDbContext context) : base(context) { }

    public async Task<CompanySubscription?> GetWithDetailsAsync(int id)
        => await _dbSet
            .Include(cs => cs.Company)
            .Include(cs => cs.SubscriptionPlan)
            .FirstOrDefaultAsync(cs => cs.Id == id);

    public async Task<IEnumerable<CompanySubscription>> GetByCompanyAsync(int companyId)
        => await _dbSet
            .Include(cs => cs.SubscriptionPlan)
            .Where(cs => cs.CompanyId == companyId)
            .OrderByDescending(cs => cs.StartDate)
            .ToListAsync();

    public async Task<CompanySubscription?> GetActiveSubscriptionAsync(int companyId)
        => await _dbSet
            .Include(cs => cs.SubscriptionPlan)
            .FirstOrDefaultAsync(cs => cs.CompanyId == companyId &&
                                       cs.Status == SubscriptionStatus.Active);

    public async Task<(IEnumerable<CompanySubscription> Items, int Total)> GetPagedAsync(
        int page, int pageSize, int? companyId = null, SubscriptionStatus? status = null)
    {
        var query = _dbSet
            .Include(cs => cs.Company)
            .Include(cs => cs.SubscriptionPlan)
            .AsQueryable();

        if (companyId.HasValue)
            query = query.Where(cs => cs.CompanyId == companyId.Value);

        if (status.HasValue)
            query = query.Where(cs => cs.Status == status.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(cs => cs.StartDate)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();
        return (items, total);
    }
}

// ── Invoice ───────────────────────────────────────────────────────────────
public class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(AppDbContext context) : base(context) { }

    public async Task<Invoice?> GetWithDetailsAsync(int id)
        => await _dbSet
            .Include(i => i.Company)
            .Include(i => i.Trade)
            .Include(i => i.CompanySubscription)
                .ThenInclude(cs => cs!.SubscriptionPlan)
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<bool> IsInvoiceNumberUniqueAsync(string invoiceNumber, int? excludeId = null)
        => !await _dbSet.AnyAsync(i => i.InvoiceNumber == invoiceNumber && i.Id != excludeId);

    public async Task<(IEnumerable<Invoice> Items, int Total)> GetPagedAsync(InvoiceFilterRequest filter)
    {
        var query = _dbSet
            .Include(i => i.Company)
            .Include(i => i.Trade)
            .Include(i => i.CompanySubscription)
                .ThenInclude(cs => cs!.SubscriptionPlan)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            query = query.Where(i => i.InvoiceNumber.Contains(filter.SearchTerm) ||
                                     (i.Company != null && i.Company.Name.Contains(filter.SearchTerm)));

        if (filter.Status.HasValue)
            query = query.Where(i => i.Status == filter.Status.Value);

        if (filter.Type.HasValue)
            query = query.Where(i => i.Type == filter.Type.Value);

        if (filter.CompanyId.HasValue)
            query = query.Where(i => i.CompanyId == filter.CompanyId.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(i => i.InvoiceDate >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(i => i.InvoiceDate <= filter.ToDate.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(i => i.InvoiceDate)
                               .Skip((filter.Page - 1) * filter.PageSize)
                               .Take(filter.PageSize)
                               .ToListAsync();
        return (items, total);
    }

    public async Task<IEnumerable<Invoice>> GetByCompanyAsync(int companyId)
        => await _dbSet
            .Include(i => i.Trade)
            .Where(i => i.CompanyId == companyId)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();

    public async Task<IEnumerable<Invoice>> GetByTradeAsync(int tradeId)
        => await _dbSet.Where(i => i.TradeId == tradeId).ToListAsync();
}

// ── User ──────────────────────────────────────────────────────────────────
public class UserRepository : GenericRepository<AppUser>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<AppUser?> GetByEmailAsync(string email)
        => await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
        => !await _dbSet.AnyAsync(u => u.Email.ToLower() == email.ToLower() && u.Id != excludeId);
}