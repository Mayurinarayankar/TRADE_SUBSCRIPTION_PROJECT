using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeSubscriptionAPI.DTOs.Request;
using TradeSubscriptionAPI.DTOs.Response;
using TradeSubscriptionAPI.Services.Interfaces;

namespace TradeSubscriptionAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _service;


    public InvoicesController(IInvoiceService service) => _service = service;

    /// <summary>Get paginated/filtered invoices</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<InvoiceResponse>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] InvoiceFilterRequest filter)
    {
        var result = await _service.GetPagedAsync(filter);
        return Ok(ApiResponse<PagedResponse<InvoiceResponse>>.Ok(result));
    }

    /// <summary>Get an invoice by ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null
            ? NotFound(ApiResponse<InvoiceResponse>.Fail($"Invoice {id} not found"))
            : Ok(ApiResponse<InvoiceResponse>.Ok(result));
    }

    /// <summary>Get all invoices for a company</summary>
    [HttpGet("by-company/{companyId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InvoiceResponse>>), 200)]
    public async Task<IActionResult> GetByCompany(int companyId)
    {
        var result = await _service.GetByCompanyAsync(companyId);
        return Ok(ApiResponse<IEnumerable<InvoiceResponse>>.Ok(result));
    }

    /// <summary>Create a new invoice (tax amount auto-calculated)</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<InvoiceResponse>), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<InvoiceResponse>.Ok(result, "Invoice created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<InvoiceResponse>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<InvoiceResponse>.Fail(ex.Message));
        }
    }

    /// <summary>Update an invoice</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceResponse>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInvoiceRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return result == null
                ? NotFound(ApiResponse<InvoiceResponse>.Fail($"Invoice {id} not found"))
                : Ok(ApiResponse<InvoiceResponse>.Ok(result, "Invoice updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<InvoiceResponse>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<InvoiceResponse>.Fail(ex.Message));
        }
    }

    /// <summary>Delete an invoice</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted
            ? Ok(ApiResponse<string>.Ok("Deleted", "Invoice deleted successfully"))
            : NotFound(ApiResponse<string>.Fail($"Invoice {id} not found"));
    }
}