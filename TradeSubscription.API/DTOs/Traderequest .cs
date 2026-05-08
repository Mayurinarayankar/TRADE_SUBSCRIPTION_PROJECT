using System.ComponentModel.DataAnnotations;
using TradeSubscriptionAPI.Models;

namespace TradeSubscriptionAPI.DTOs.Request;

public class CreateTradeRequest
{
    [Required, MaxLength(50)]
    public string TradeNumber { get; set; } = string.Empty;

    [Required]
    public DateTime TradeDate { get; set; }

    public DateTime? ShipmentDate { get; set; }
    public DateTime? DeliveryDate { get; set; }

    [Required]
    public int CompanyId { get; set; }

    [Required]
    public int IncotermId { get; set; }

    [Required, MaxLength(100)]
    public string Commodity { get; set; } = string.Empty;

    [Range(0.0001, double.MaxValue)]
    public decimal Quantity { get; set; }

    [MaxLength(20)]
    public string? Unit { get; set; }

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    [MaxLength(200)]
    public string? PortOfLoading { get; set; }

    [MaxLength(200)]
    public string? PortOfDischarge { get; set; }

    [MaxLength(200)]
    public string? CountryOfOrigin { get; set; }

    [MaxLength(1000)]
    public string? Remarks { get; set; }
}

public class UpdateTradeRequest : CreateTradeRequest
{
    public TradeStatus Status { get; set; } = TradeStatus.Draft;
}

public class TradeFilterRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public TradeStatus? Status { get; set; }
    public int? CompanyId { get; set; }
    public int? IncotermId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Currency { get; set; }
}