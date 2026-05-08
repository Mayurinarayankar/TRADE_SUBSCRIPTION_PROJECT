using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeSubscriptionAPI.Models;

public class SubscriptionPlan : BaseEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MonthlyPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AnnualPrice { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    public int MaxUsers { get; set; } = 1;
    public int MaxTrades { get; set; } = 100;   // -1 = unlimited
    public bool HasApiAccess { get; set; } = false;
    public bool HasReporting { get; set; } = false;
    public bool HasAdvancedAnalytics { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public PlanTier Tier { get; set; } = PlanTier.Basic;

    // Navigation
    public ICollection<CompanySubscription> Subscriptions { get; set; } = new List<CompanySubscription>();
}

public enum PlanTier
{
    Free = 0,
    Basic = 1,
    Professional = 2,
    Enterprise = 3
}