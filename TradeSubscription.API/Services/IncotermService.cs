using TradeSubscriptionAPI.DTOs.Request;
using TradeSubscriptionAPI.DTOs.Response;
using TradeSubscriptionAPI.Models;
using TradeSubscriptionAPI.Repositories.Interfaces;
using TradeSubscriptionAPI.Services.Interfaces;

namespace TradeSubscriptionAPI.Services;
public class IncotermService : IIncotermService
{
    private readonly IIncotermRepository _repo;

    public IncotermService(IIncotermRepository repo) => _repo = repo;

    public async Task<IEnumerable<IncotermResponse>> GetAllAsync()
        => (await _repo.GetAllAsync()).Select(Map);

    public async Task<IEnumerable<IncotermResponse>> GetActiveAsync()
        => (await _repo.GetActiveAsync()).Select(Map);

    public async Task<IncotermResponse?> GetByIdAsync(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item == null ? null : Map(item);
    }

    public async Task<IncotermResponse> CreateAsync(CreateIncotermRequest request)
    {
        request.Code = request.Code.ToUpper();
        if (!await _repo.IsCodeUniqueAsync(request.Code))
            throw new InvalidOperationException($"Incoterm code '{request.Code}' already exists.");

        var incoterm = new Incoterm
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            TransportMode = request.TransportMode,
            IsActive = request.IsActive
        };

        await _repo.AddAsync(incoterm);
        return Map(incoterm);
    }

    public async Task<IncotermResponse?> UpdateAsync(int id, UpdateIncotermRequest request)
    {
        var incoterm = await _repo.GetByIdAsync(id);
        if (incoterm == null) return null;

        request.Code = request.Code.ToUpper();
        if (!await _repo.IsCodeUniqueAsync(request.Code, id))
            throw new InvalidOperationException($"Incoterm code '{request.Code}' already exists.");

        incoterm.Code = request.Code;
        incoterm.Name = request.Name;
        incoterm.Description = request.Description;
        incoterm.TransportMode = request.TransportMode;
        incoterm.IsActive = request.IsActive;

        await _repo.UpdateAsync(incoterm);
        return Map(incoterm);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return false;
        await _repo.DeleteAsync(item);
        return true;
    }

    private static IncotermResponse Map(Incoterm i) => new()
    {
        Id = i.Id,
        Code = i.Code,
        Name = i.Name,
        Description = i.Description,
        TransportMode = i.TransportMode.ToString(),
        IsActive = i.IsActive
    };
}
