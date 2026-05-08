using TradeSubscriptionAPI.DTOs.Request;
using TradeSubscriptionAPI.DTOs.Response;
using TradeSubscriptionAPI.Models;
using TradeSubscriptionAPI.Repositories.Interfaces;
using TradeSubscriptionAPI.Services.Interfaces;

namespace TradeSubscriptionAPI.Services;
public class TradeService : ITradeService
{
    private readonly ITradeRepository _repo;
    private readonly ICompanyRepository _companyRepo;
    private readonly IIncotermRepository _incotermRepo;

    public TradeService(ITradeRepository repo, ICompanyRepository companyRepo, IIncotermRepository incotermRepo)
    {
        _repo = repo;
        _companyRepo = companyRepo;
        _incotermRepo = incotermRepo;
    }

    public async Task<PagedResponse<TradeResponse>> GetPagedAsync(TradeFilterRequest filter)
    {
        var (items, total) = await _repo.GetPagedAsync(filter);
        return new PagedResponse<TradeResponse>
        {
            Items = items.Select(Map).ToList(),
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<TradeResponse?> GetByIdAsync(int id)
    {
        var trade = await _repo.GetWithDetailsAsync(id);
        return trade == null ? null : Map(trade);
    }

    public async Task<TradeResponse> CreateAsync(CreateTradeRequest request)
    {
        if (!await _repo.IsTradeNumberUniqueAsync(request.TradeNumber))
            throw new InvalidOperationException($"Trade number '{request.TradeNumber}' already exists.");

        if (!await _companyRepo.ExistsAsync(request.CompanyId))
            throw new KeyNotFoundException($"Company {request.CompanyId} not found.");

        if (!await _incotermRepo.ExistsAsync(request.IncotermId))
            throw new KeyNotFoundException($"Incoterm {request.IncotermId} not found.");

        var trade = new Trade
        {
            TradeNumber = request.TradeNumber,
            TradeDate = request.TradeDate,
            ShipmentDate = request.ShipmentDate,
            DeliveryDate = request.DeliveryDate,
            CompanyId = request.CompanyId,
            IncotermId = request.IncotermId,
            Commodity = request.Commodity,
            Quantity = request.Quantity,
            Unit = request.Unit,
            UnitPrice = request.UnitPrice,
            Currency = request.Currency,
            TotalAmount = Math.Round(request.Quantity * request.UnitPrice, 2),
            PortOfLoading = request.PortOfLoading,
            PortOfDischarge = request.PortOfDischarge,
            CountryOfOrigin = request.CountryOfOrigin,
            Remarks = request.Remarks,
            Status = TradeStatus.Draft
        };

        await _repo.AddAsync(trade);
        var created = await _repo.GetWithDetailsAsync(trade.Id);
        return Map(created!);
    }

    public async Task<TradeResponse?> UpdateAsync(int id, UpdateTradeRequest request)
    {
        var trade = await _repo.GetWithDetailsAsync(id);
        if (trade == null) return null;

        if (!await _repo.IsTradeNumberUniqueAsync(request.TradeNumber, id))
            throw new InvalidOperationException($"Trade number '{request.TradeNumber}' already exists.");

        if (!await _companyRepo.ExistsAsync(request.CompanyId))
            throw new KeyNotFoundException($"Company {request.CompanyId} not found.");

        if (!await _incotermRepo.ExistsAsync(request.IncotermId))
            throw new KeyNotFoundException($"Incoterm {request.IncotermId} not found.");

        trade.TradeNumber = request.TradeNumber;
        trade.TradeDate = request.TradeDate;
        trade.ShipmentDate = request.ShipmentDate;
        trade.DeliveryDate = request.DeliveryDate;
        trade.CompanyId = request.CompanyId;
        trade.IncotermId = request.IncotermId;
        trade.Commodity = request.Commodity;
        trade.Quantity = request.Quantity;
        trade.Unit = request.Unit;
        trade.UnitPrice = request.UnitPrice;
        trade.Currency = request.Currency;
        trade.TotalAmount = Math.Round(request.Quantity * request.UnitPrice, 2);
        trade.PortOfLoading = request.PortOfLoading;
        trade.PortOfDischarge = request.PortOfDischarge;
        trade.CountryOfOrigin = request.CountryOfOrigin;
        trade.Remarks = request.Remarks;
        trade.Status = request.Status;

        await _repo.UpdateAsync(trade);
        var updated = await _repo.GetWithDetailsAsync(trade.Id);
        return Map(updated!);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var trade = await _repo.GetByIdAsync(id);
        if (trade == null) return false;
        await _repo.DeleteAsync(trade);
        return true;
    }

    public async Task<IEnumerable<TradeResponse>> GetByCompanyAsync(int companyId)
        => (await _repo.GetByCompanyAsync(companyId)).Select(Map);

    private static TradeResponse Map(Trade t) => new()
    {
        Id = t.Id,
        TradeNumber = t.TradeNumber,
        TradeDate = t.TradeDate,
        ShipmentDate = t.ShipmentDate,
        DeliveryDate = t.DeliveryDate,
        CompanyId = t.CompanyId,
        CompanyName = t.Company?.Name ?? string.Empty,
        IncotermId = t.IncotermId,
        IncotermCode = t.Incoterm?.Code ?? string.Empty,
        Commodity = t.Commodity,
        Quantity = t.Quantity,
        Unit = t.Unit,
        UnitPrice = t.UnitPrice,
        Currency = t.Currency,
        TotalAmount = t.TotalAmount,
        PortOfLoading = t.PortOfLoading,
        PortOfDischarge = t.PortOfDischarge,
        CountryOfOrigin = t.CountryOfOrigin,
        Status = t.Status.ToString(),
        Remarks = t.Remarks,
        CreatedAt = t.CreatedAt
    };
}