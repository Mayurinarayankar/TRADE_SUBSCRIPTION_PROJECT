using TradeSubscriptionAPI.DTOs.Request;
using TradeSubscriptionAPI.Models;

namespace TradeSubscriptionAPI.Repositories.Interfaces;

public interface ICompanyRepository : IGenericRepository<Company>
{
    Task<Company?> GetByNameAsync(string name);
    Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
    Task<(IEnumerable<Company> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search = null, CompanyType? type = null);
}

public interface IIncotermRepository : IGenericRepository<Incoterm>
{
    Task<Incoterm?> GetByCodeAsync(string code);
    Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
    Task<IEnumerable<Incoterm>> GetActiveAsync();
}

public interface ITradeRepository : IGenericRepository<Trade>
{
    Task<Trade?> GetWithDetailsAsync(int id);
    Task<bool> IsTradeNumberUniqueAsync(string tradeNumber, int? excludeId = null);
    Task<(IEnumerable<Trade> Items, int Total)> GetPagedAsync(TradeFilterRequest filter);
    Task<IEnumerable<Trade>> GetByCompanyAsync(int companyId);
}

public interface ISubscriptionPlanRepository : IGenericRepository<SubscriptionPlan>
{
    Task<IEnumerable<SubscriptionPlan>> GetActiveAsync();
    Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
}

public interface ICompanySubscriptionRepository : IGenericRepository<CompanySubscription>
{
    Task<CompanySubscription?> GetWithDetailsAsync(int id);
    Task<IEnumerable<CompanySubscription>> GetByCompanyAsync(int companyId);
    Task<CompanySubscription?> GetActiveSubscriptionAsync(int companyId);
    Task<(IEnumerable<CompanySubscription> Items, int Total)> GetPagedAsync(int page, int pageSize, int? companyId = null, SubscriptionStatus? status = null);
}

public interface IInvoiceRepository : IGenericRepository<Invoice>
{
    Task<Invoice?> GetWithDetailsAsync(int id);
    Task<bool> IsInvoiceNumberUniqueAsync(string invoiceNumber, int? excludeId = null);
    Task<(IEnumerable<Invoice> Items, int Total)> GetPagedAsync(InvoiceFilterRequest filter);
    Task<IEnumerable<Invoice>> GetByCompanyAsync(int companyId);
    Task<IEnumerable<Invoice>> GetByTradeAsync(int tradeId);
}

public interface IUserRepository : IGenericRepository<AppUser>
{
    Task<AppUser?> GetByEmailAsync(string email);
    Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null);
}