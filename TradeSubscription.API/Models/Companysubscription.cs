using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeSubscriptionAPI.Models;

public class CompanySubscription : BaseEntity
{
    [Required]
    public int CompanyId { get; set; }
    public Company? Company { get; set; }

    [Required]
    public int SubscriptionPlanId { get; set; }
    public SubscriptionPlan? SubscriptionPlan { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;

    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    public bool AutoRenew { get; set; } = true;

    [MaxLength(200)]
    public string? Notes { get; set; }

    // Navigation
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

public enum BillingCycle
{
    Monthly = 1,
    Annual = 2
}

public enum SubscriptionStatus
{
    Active = 1,
    Expired = 2,
    Cancelled = 3,
    Suspended = 4,
    Trial = 5
}