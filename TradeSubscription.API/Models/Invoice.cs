using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeSubscriptionAPI.Models;

public class Invoice : BaseEntity
{
    [Required, MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    public DateTime InvoiceDate { get; set; }

    public DateTime? DueDate { get; set; }

    [Required]
    public int CompanyId { get; set; }
    public Company? Company { get; set; }

    // Either linked to a Trade or a Subscription
    public int? TradeId { get; set; }
    public Trade? Trade { get; set; }

    public int? CompanySubscriptionId { get; set; }
    public CompanySubscription? CompanySubscription { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal TaxRate { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    public InvoiceType Type { get; set; } = InvoiceType.Trade;
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime? PaidAt { get; set; }

    [MaxLength(100)]
    public string? PaymentReference { get; set; }
}

public enum InvoiceType
{
    Trade = 1,
    Subscription = 2
}

public enum InvoiceStatus
{
    Draft = 1,
    Sent = 2,
    Paid = 3,
    Overdue = 4,
    Cancelled = 5,
    Refunded = 6
}