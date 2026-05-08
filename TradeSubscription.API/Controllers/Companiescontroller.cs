using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeSubscriptionAPI.DTOs.Request;
using TradeSubscriptionAPI.DTOs.Response;
using TradeSubscriptionAPI.Models;
using TradeSubscriptionAPI.Services.Interfaces;

namespace TradeSubscriptionAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _service;

    public CompaniesController(ICompanyService service) => _service = service;

    /// <summary>Get paginated list of companies</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<CompanyResponse>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] CompanyType? type = null)
    {
        var result = await _service.GetPagedAsync(page, pageSize, search, type);
        return Ok(ApiResponse<PagedResponse<CompanyResponse>>.Ok(result));
    }

    /// <summary>Get a company by ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CompanyResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null
            ? NotFound(ApiResponse<CompanyResponse>.Fail($"Company {id} not found"))
            : Ok(ApiResponse<CompanyResponse>.Ok(result));
    }

    /// <summary>Create a new company</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CompanyResponse>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateCompanyRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<CompanyResponse>.Ok(result, "Company created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CompanyResponse>.Fail(ex.Message));
        }
    }

    /// <summary>Update an existing company</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CompanyResponse>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCompanyRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return result == null
                ? NotFound(ApiResponse<CompanyResponse>.Fail($"Company {id} not found"))
                : Ok(ApiResponse<CompanyResponse>.Ok(result, "Company updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CompanyResponse>.Fail(ex.Message));
        }
    }

    /// <summary>Soft delete a company</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted
            ? Ok(ApiResponse<string>.Ok("Deleted", "Company deleted successfully"))
            : NotFound(ApiResponse<string>.Fail($"Company {id} not found"));
    }
}