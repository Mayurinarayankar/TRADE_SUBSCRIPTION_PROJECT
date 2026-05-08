using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeSubscriptionAPI.DTOs.Request;
using TradeSubscriptionAPI.DTOs.Response;
using TradeSubscriptionAPI.Models;
using TradeSubscriptionAPI.Services.Interfaces;

namespace TradeSubscriptionAPI.Controllers;

// ── Subscription Plans ────────────────────────────────────────────────────
[ApiController]
[Route("api/subscription-plans")]
[Authorize]
[Produces("application/json")]
public class SubscriptionPlansController : ControllerBase
{
    private readonly ISubscriptionPlanService _service;

    public SubscriptionPlansController(ISubscriptionPlanService service) => _service = service;

    /// <summary>Get all subscription plans</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SubscriptionPlanResponse>>), 200)]
    public async Task<IActionResult> GetAll()
        => Ok(ApiResponse<IEnumerable<SubscriptionPlanResponse>>.Ok(await _service.GetAllAsync()));

    /// <summary>Get active subscription plans</summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SubscriptionPlanResponse>>), 200)]
    public async Task<IActionResult> GetActive()
        => Ok(ApiResponse<IEnumerable<SubscriptionPlanResponse>>.Ok(await _service.GetActiveAsync()));

    /// <summary>Get a subscription plan by ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionPlanResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null
            ? NotFound(ApiResponse<SubscriptionPlanResponse>.Fail($"Plan {id} not found"))
            : Ok(ApiResponse<SubscriptionPlanResponse>.Ok(result));
    }

    /// <summary>Create a new subscription plan</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionPlanResponse>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateSubscriptionPlanRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<SubscriptionPlanResponse>.Ok(result, "Plan created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SubscriptionPlanResponse>.Fail(ex.Message));
        }
    }

    /// <summary>Update a subscription plan</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionPlanResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSubscriptionPlanRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return result == null
                ? NotFound(ApiResponse<SubscriptionPlanResponse>.Fail($"Plan {id} not found"))
                : Ok(ApiResponse<SubscriptionPlanResponse>.Ok(result, "Plan updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<SubscriptionPlanResponse>.Fail(ex.Message));
        }
    }

    /// <summary>Delete a subscription plan</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted
            ? Ok(ApiResponse<string>.Ok("Deleted", "Plan deleted successfully"))
            : NotFound(ApiResponse<string>.Fail($"Plan {id} not found"));
    }
}

// ── Company Subscriptions ─────────────────────────────────────────────────
[ApiController]
[Route("api/subscriptions")]
[Authorize]
[Produces("application/json")]
public class CompanySubscriptionsController : ControllerBase
{
    private readonly ICompanySubscriptionService _service;

    public CompanySubscriptionsController(ICompanySubscriptionService service) => _service = service;

    /// <summary>Get paginated subscriptions</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<CompanySubscriptionResponse>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? companyId = null,
        [FromQuery] SubscriptionStatus? status = null)
    {
        var result = await _service.GetPagedAsync(page, pageSize, companyId, status);
        return Ok(ApiResponse<PagedResponse<CompanySubscriptionResponse>>.Ok(result));
    }

    /// <summary>Get a subscription by ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CompanySubscriptionResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null
            ? NotFound(ApiResponse<CompanySubscriptionResponse>.Fail($"Subscription {id} not found"))
            : Ok(ApiResponse<CompanySubscriptionResponse>.Ok(result));
    }

    /// <summary>Get all subscriptions for a company</summary>
    [HttpGet("by-company/{companyId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CompanySubscriptionResponse>>), 200)]
    public async Task<IActionResult> GetByCompany(int companyId)
    {
        var result = await _service.GetByCompanyAsync(companyId);
        return Ok(ApiResponse<IEnumerable<CompanySubscriptionResponse>>.Ok(result));
    }

    /// <summary>Subscribe a company to a plan</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CompanySubscriptionResponse>), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Create([FromBody] CreateCompanySubscriptionRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<CompanySubscriptionResponse>.Ok(result, "Subscription created successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CompanySubscriptionResponse>.Fail(ex.Message));
        }
    }

    /// <summary>Update a subscription</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CompanySubscriptionResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCompanySubscriptionRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return result == null
                ? NotFound(ApiResponse<CompanySubscriptionResponse>.Fail($"Subscription {id} not found"))
                : Ok(ApiResponse<CompanySubscriptionResponse>.Ok(result, "Subscription updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CompanySubscriptionResponse>.Fail(ex.Message));
        }
    }

    /// <summary>Cancel a subscription</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted
            ? Ok(ApiResponse<string>.Ok("Deleted", "Subscription cancelled successfully"))
            : NotFound(ApiResponse<string>.Fail($"Subscription {id} not found"));
    }
}