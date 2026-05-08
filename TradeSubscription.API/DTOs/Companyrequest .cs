using System.ComponentModel.DataAnnotations;
using TradeSubscriptionAPI.Models;

namespace TradeSubscriptionAPI.DTOs.Request;

public class CreateCompanyRequest
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
}

public class UpdateCompanyRequest : CreateCompanyRequest { }