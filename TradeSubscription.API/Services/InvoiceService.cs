using TradeSubscriptionAPI.DTOs.Request;
using TradeSubscriptionAPI.DTOs.Response;
using TradeSubscriptionAPI.Models;
using TradeSubscriptionAPI.Repositories.Interfaces;
using TradeSubscriptionAPI.Services.Interfaces;

namespace TradeSubscriptionAPI.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _repo;
    private readonly ICompanyRepository _companyRepo;

    public InvoiceService(IInvoiceRepository repo, ICompanyRepository companyRepo)
    {
        _repo = repo;
        _companyRepo = companyRepo;
    }

    public async Task<PagedResponse<InvoiceResponse>> GetPagedAsync(InvoiceFilterRequest filter)
    {
        var (items, total) = await _repo.GetPagedAsync(filter);
        return new PagedResponse<InvoiceResponse>
        {
            Items = items.Select(Map).ToList(),
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<InvoiceResponse?> GetByIdAsync(int id)
    {
        var invoice = await _repo.GetWithDetailsAsync(id);
        return invoice == null ? null : Map(invoice);
    }

    public async Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request)
    {
        if (!await _repo.IsInvoiceNumberUniqueAsync(request.InvoiceNumber))
            throw new InvalidOperationException($"Invoice number '{request.InvoiceNumber}' already exists.");

        if (!await _companyRepo.ExistsAsync(request.CompanyId))
            throw new KeyNotFoundException($"Company {request.CompanyId} not found.");

        var taxAmount = Math.Round(request.SubTotal * (request.TaxRate / 100), 2);

        var invoice = new Invoice
        {
            InvoiceNumber = request.InvoiceNumber,
            InvoiceDate = request.InvoiceDate,
            DueDate = request.DueDate,
            CompanyId = request.CompanyId,
            TradeId = request.TradeId,
            CompanySubscriptionId = request.CompanySubscriptionId,
            SubTotal = request.SubTotal,
            TaxRate = request.TaxRate,
            TaxAmount = taxAmount,
            TotalAmount = Math.Round(request.SubTotal + taxAmount, 2),
            Currency = request.Currency,
            Type = request.Type,
            Notes = request.Notes,
            Status = InvoiceStatus.Draft
        };

        await _repo.AddAsync(invoice);
        var created = await _repo.GetWithDetailsAsync(invoice.Id);
        return Map(created!);
    }

    public async Task<InvoiceResponse?> UpdateAsync(int id, UpdateInvoiceRequest request)
    {
        var invoice = await _repo.GetWithDetailsAsync(id);
        if (invoice == null) return null;

        if (!await _repo.IsInvoiceNumberUniqueAsync(request.InvoiceNumber, id))
            throw new InvalidOperationException($"Invoice number '{request.InvoiceNumber}' already exists.");

        if (!await _companyRepo.ExistsAsync(request.CompanyId))
            throw new KeyNotFoundException($"Company {request.CompanyId} not found.");

        var taxAmount = Math.Round(request.SubTotal * (request.TaxRate / 100), 2);

        invoice.InvoiceNumber = request.InvoiceNumber;
        invoice.InvoiceDate = request.InvoiceDate;
        invoice.DueDate = request.DueDate;
        invoice.CompanyId = request.CompanyId;
        invoice.TradeId = request.TradeId;
        invoice.CompanySubscriptionId = request.CompanySubscriptionId;
        invoice.SubTotal = request.SubTotal;
        invoice.TaxRate = request.TaxRate;
        invoice.TaxAmount = taxAmount;
        invoice.TotalAmount = Math.Round(request.SubTotal + taxAmount, 2);
        invoice.Currency = request.Currency;
        invoice.Type = request.Type;
        invoice.Notes = request.Notes;
        invoice.Status = request.Status;
        invoice.PaidAt = request.PaidAt;
        invoice.PaymentReference = request.PaymentReference;

        await _repo.UpdateAsync(invoice);
        var updated = await _repo.GetWithDetailsAsync(invoice.Id);
        return Map(updated!);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var invoice = await _repo.GetByIdAsync(id);
        if (invoice == null) return false;
        await _repo.DeleteAsync(invoice);
        return true;
    }

    public async Task<IEnumerable<InvoiceResponse>> GetByCompanyAsync(int companyId)
        => (await _repo.GetByCompanyAsync(companyId)).Select(Map);

    private static InvoiceResponse Map(Invoice i) => new()
    {
        Id = i.Id,
        InvoiceNumber = i.InvoiceNumber,
        InvoiceDate = i.InvoiceDate,
        DueDate = i.DueDate,
        CompanyId = i.CompanyId,
        CompanyName = i.Company?.Name ?? string.Empty,
        TradeId = i.TradeId,
        TradeNumber = i.Trade?.TradeNumber,
        CompanySubscriptionId = i.CompanySubscriptionId,
        PlanName = i.CompanySubscription?.SubscriptionPlan?.Name,
        SubTotal = i.SubTotal,
        TaxRate = i.TaxRate,
        TaxAmount = i.TaxAmount,
        TotalAmount = i.TotalAmount,
        Currency = i.Currency,
        Type = i.Type.ToString(),
        Status = i.Status.ToString(),
        Notes = i.Notes,
        PaidAt = i.PaidAt,
        PaymentReference = i.PaymentReference,
        CreatedAt = i.CreatedAt
    };
}