using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

/// <summary>
/// Demo checkout request: cart id plus optional shipping info. No real payment.
/// </summary>
public class CheckoutRequestDto
{
    [Required]
    public required string CartId { get; set; }

    public CheckoutUserInfoDto? UserInfo { get; set; }
    public AddressDto? ShippingAddress { get; set; }
}

public class CheckoutUserInfoDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
}
