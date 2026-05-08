using System.ComponentModel.DataAnnotations;
using TradeSubscriptionAPI.Models;

namespace TradeSubscriptionAPI.DTOs.Request;

public class CreateIncotermRequest
{
    [Required, MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public TransportMode TransportMode { get; set; } = TransportMode.Any;

    public bool IsActive { get; set; } = true;
}

public class UpdateIncotermRequest : CreateIncotermRequest { }