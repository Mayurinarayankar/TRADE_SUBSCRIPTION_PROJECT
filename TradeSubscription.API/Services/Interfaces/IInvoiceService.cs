using TradeSubscriptionAPI.DTOs.Request;
using TradeSubscriptionAPI.DTOs.Response;

namespace TradeSubscriptionAPI.Services.Interfaces;

public interface IInvoiceService
{
    Task<PagedResponse<InvoiceResponse>> GetPagedAsync(InvoiceFilterRequest filter);
    Task<InvoiceResponse?> GetByIdAsync(int id);
    Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request);
    Task<InvoiceResponse?> UpdateAsync(int id, UpdateInvoiceRequest request);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<InvoiceResponse>> GetByCompanyAsync(int companyId);
}