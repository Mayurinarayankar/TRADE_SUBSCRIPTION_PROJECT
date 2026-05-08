using System.ComponentModel.DataAnnotations;
using TradeSubscriptionAPI.Models;

namespace TradeSubscriptionAPI.DTOs.Request;

// ── Subscription Plan ──────────────────────────────────────────────────────
public class CreateSubscriptionPlanRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MonthlyPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal AnnualPrice { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    public int MaxUsers { get; set; } = 1;
    public int MaxTrades { get; set; } = 100;
    public bool HasApiAccess { get; set; } = false;
    public bool HasReporting { get; set; } = false;
    public bool HasAdvancedAnalytics { get; set; } = false;
    public PlanTier Tier { get; set; } = PlanTier.Basic;
    public bool IsActive { get; set; } = true;
}

public class UpdateSubscriptionPlanRequest : CreateSubscriptionPlanRequest { }

// ── Company Subscription ───────────────────────────────────────────────────
public class CreateCompanySubscriptionRequest
{
    [Required]
    public int CompanyId { get; set; }

    [Required]
    public int SubscriptionPlanId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;

    [Range(0, double.MaxValue)]
    public decimal AmountPaid { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    public bool AutoRenew { get; set; } = true;

    [MaxLength(200)]
    public string? Notes { get; set; }
}

public class UpdateCompanySubscriptionRequest : CreateCompanySubscriptionRequest
{
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
}

// ── Invoice ────────────────────────────────────────────────────────────────
public class CreateInvoiceRequest
{
    [Required, MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    public DateTime InvoiceDate { get; set; }

    public DateTime? DueDate { get; set; }

    [Required]
    public int CompanyId { get; set; }

    public int? TradeId { get; set; }
    public int? CompanySubscriptionId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal SubTotal { get; set; }

    [Range(0, 100)]
    public decimal TaxRate { get; set; } = 0;

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    public InvoiceType Type { get; set; } = InvoiceType.Trade;

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class UpdateInvoiceRequest : CreateInvoiceRequest
{
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public DateTime? PaidAt { get; set; }

    [MaxLength(100)]
    public string? PaymentReference { get; set; }
}

public class InvoiceFilterRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public InvoiceStatus? Status { get; set; }
    public InvoiceType? Type { get; set; }
    public int? CompanyId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}