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
public class IncotermsController : ControllerBase
{
    private readonly IIncotermService _service;

    public IncotermsController(IIncotermService service) => _service = service;

    /// <summary>Get all incoterms</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<IncotermResponse>>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<IncotermResponse>>.Ok(result));
    }

    /// <summary>Get active incoterms only</summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<IncotermResponse>>), 200)]
    public async Task<IActionResult> GetActive()
    {
        var result = await _service.GetActiveAsync();
        return Ok(ApiResponse<IEnumerable<IncotermResponse>>.Ok(result));
    }

    /// <summary>Get an incoterm by ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<IncotermResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null
            ? NotFound(ApiResponse<IncotermResponse>.Fail($"Incoterm {id} not found"))
            : Ok(ApiResponse<IncotermResponse>.Ok(result));
    }

    /// <summary>Create a new incoterm</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<IncotermResponse>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateIncotermRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<IncotermResponse>.Ok(result, "Incoterm created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<IncotermResponse>.Fail(ex.Message));
        }
    }

    /// <summary>Update an incoterm</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<IncotermResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateIncotermRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return result == null
                ? NotFound(ApiResponse<IncotermResponse>.Fail($"Incoterm {id} not found"))
                : Ok(ApiResponse<IncotermResponse>.Ok(result, "Incoterm updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<IncotermResponse>.Fail(ex.Message));
        }
    }

    /// <summary>Delete an incoterm</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted
            ? Ok(ApiResponse<string>.Ok("Deleted", "Incoterm deleted successfully"))
            : NotFound(ApiResponse<string>.Fail($"Incoterm {id} not found"));
    }
}