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
public class TradesController : ControllerBase
{
    private readonly ITradeService _service;

    public TradesController(ITradeService service) => _service = service;

    /// <summary>Get paginated/filtered trades</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<TradeResponse>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] TradeFilterRequest filter)
    {
        var result = await _service.GetPagedAsync(filter);
        return Ok(ApiResponse<PagedResponse<TradeResponse>>.Ok(result));
    }

    /// <summary>Get a trade by ID (with company and incoterm details)</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TradeResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null
            ? NotFound(ApiResponse<TradeResponse>.Fail($"Trade {id} not found"))
            : Ok(ApiResponse<TradeResponse>.Ok(result));
    }

    /// <summary>Get all trades for a specific company</summary>
    [HttpGet("by-company/{companyId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TradeResponse>>), 200)]
    public async Task<IActionResult> GetByCompany(int companyId)
    {
        var result = await _service.GetByCompanyAsync(companyId);
        return Ok(ApiResponse<IEnumerable<TradeResponse>>.Ok(result));
    }

    /// <summary>Create a new trade</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TradeResponse>), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Create([FromBody] CreateTradeRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<TradeResponse>.Ok(result, "Trade created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<TradeResponse>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TradeResponse>.Fail(ex.Message));
        }
    }

    /// <summary>Update an existing trade</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TradeResponse>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTradeRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return result == null
                ? NotFound(ApiResponse<TradeResponse>.Fail($"Trade {id} not found"))
                : Ok(ApiResponse<TradeResponse>.Ok(result, "Trade updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<TradeResponse>.Fail(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TradeResponse>.Fail(ex.Message));
        }
    }

    /// <summary>Soft delete a trade</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted
            ? Ok(ApiResponse<string>.Ok("Deleted", "Trade deleted successfully"))
            : NotFound(ApiResponse<string>.Fail($"Trade {id} not found"));
    }
}