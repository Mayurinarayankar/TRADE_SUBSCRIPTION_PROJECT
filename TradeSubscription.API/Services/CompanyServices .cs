using TradeSubscriptionAPI.DTOs.Request;
using TradeSubscriptionAPI.DTOs.Response;
using TradeSubscriptionAPI.Models;
using TradeSubscriptionAPI.Repositories.Interfaces;
using TradeSubscriptionAPI.Services.Interfaces;

namespace TradeSubscriptionAPI.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _repo;

    public CompanyService(ICompanyRepository repo) => _repo = repo;

    public async Task<PagedResponse<CompanyResponse>> GetPagedAsync(
        int page, int pageSize, string? search, CompanyType? type)
    {
        var (items, total) = await _repo.GetPagedAsync(page, pageSize, search, type);
        return new PagedResponse<CompanyResponse>
        {
            Items = items.Select(Map).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CompanyResponse?> GetByIdAsync(int id)
    {
        var company = await _repo.GetByIdAsync(id);
        return company == null ? null : Map(company);
    }

    public async Task<CompanyResponse> CreateAsync(CreateCompanyRequest request)
    {
        if (!await _repo.IsNameUniqueAsync(request.Name))
            throw new InvalidOperationException($"Company '{request.Name}' already exists.");

        var company = new Company
        {
            Name = request.Name,
            RegistrationNumber = request.RegistrationNumber,
            TaxId = request.TaxId,
            Address = request.Address,
            City = request.City,
            Country = request.Country,
            Phone = request.Phone,
            Email = request.Email,
            Type = request.Type
        };

        await _repo.AddAsync(company);
        return Map(company);
    }

    public async Task<CompanyResponse?> UpdateAsync(int id, UpdateCompanyRequest request)
    {
        var company = await _repo.GetByIdAsync(id);
        if (company == null) return null;

        if (!await _repo.IsNameUniqueAsync(request.Name, id))
            throw new InvalidOperationException($"Company '{request.Name}' already exists.");

        company.Name = request.Name;
        company.RegistrationNumber = request.RegistrationNumber;
        company.TaxId = request.TaxId;
        company.Address = request.Address;
        company.City = request.City;
        company.Country = request.Country;
        company.Phone = request.Phone;
        company.Email = request.Email;
        company.Type = request.Type;

        await _repo.UpdateAsync(company);
        return Map(company);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var company = await _repo.GetByIdAsync(id);
        if (company == null) return false;
        await _repo.DeleteAsync(company);
        return true;
    }

    private static CompanyResponse Map(Company c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        RegistrationNumber = c.RegistrationNumber,
        TaxId = c.TaxId,
        Address = c.Address,
        City = c.City,
        Country = c.Country,
        Phone = c.Phone,
        Email = c.Email,
        Type = c.Type.ToString(),
        CreatedAt = c.CreatedAt
    };
}