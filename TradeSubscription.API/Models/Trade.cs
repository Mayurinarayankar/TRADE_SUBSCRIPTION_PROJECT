using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeSubscriptionAPI.Models;

public class Trade : BaseEntity
{
    [Required, MaxLength(50)]
    public string TradeNumber { get; set; } = string.Empty;

    [Required]
    public DateTime TradeDate { get; set; }

    public DateTime? ShipmentDate { get; set; }
    public DateTime? DeliveryDate { get; set; }

    [Required]
    public int CompanyId { get; set; }
    public Company? Company { get; set; }

    [Required]
    public int IncotermId { get; set; }
    public Incoterm? Incoterm { get; set; }

    [Required, MaxLength(100)]
    public string Commodity { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    [MaxLength(20)]
    public string? Unit { get; set; }  // MT, KG, CBM, etc.

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [MaxLength(200)]
    public string? PortOfLoading { get; set; }

    [MaxLength(200)]
    public string? PortOfDischarge { get; set; }

    [MaxLength(200)]
    public string? CountryOfOrigin { get; set; }

    public TradeStatus Status { get; set; } = TradeStatus.Draft;

    [MaxLength(1000)]
    public string? Remarks { get; set; }

    // Navigation
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

public enum TradeStatus
{
    Draft = 1,
    Confirmed = 2,
    InTransit = 3,
    Delivered = 4,
    Completed = 5,
    Cancelled = 6
}