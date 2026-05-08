using System.ComponentModel.DataAnnotations;

namespace TradeSubscriptionAPI.Models;

public class Company : BaseEntity
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? RegistrationNumber { get; set; }

    [MaxLength(100)]
    public string? TaxId { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(150), EmailAddress]
    public string? Email { get; set; }

    public CompanyType Type { get; set; } = CompanyType.Client;

    // Navigation
    public ICollection<Trade> Trades { get; set; } = new List<Trade>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<CompanySubscription> Subscriptions { get; set; } = new List<CompanySubscription>();
}

public enum CompanyType
{
    Client = 1,
    Supplier = 2,
    Both = 3
}