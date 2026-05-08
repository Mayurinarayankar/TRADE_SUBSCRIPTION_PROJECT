using System.ComponentModel.DataAnnotations;

namespace TradeSubscriptionAPI.Models;

public class Incoterm : BaseEntity
{
    [Required, MaxLength(10)]
    public string Code { get; set; } = string.Empty;   // EXW, FOB, CIF, etc.

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public TransportMode TransportMode { get; set; } = TransportMode.Any;

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Trade> Trades { get; set; } = new List<Trade>();
}

public enum TransportMode
{
    Any = 1,
    SeaAndInlandWaterway = 2,
    AirFreight = 3,
    RoadFreight = 4,
    RailFreight = 5,
    Multimodal = 6
}