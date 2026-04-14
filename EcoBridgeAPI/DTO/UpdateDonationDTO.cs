using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EcoBridgeAPI.DTO;

public class UpdateDonationDTO
{
    [StringLength(100)]
    public string? FoodType { get; set; }

    [StringLength(50)]
    public string? Quantity { get; set; }

    public DateTime? ExpiryDate { get; set; }

    [StringLength(300)]
    public string? PickupLocation { get; set; }

    public IFormFile? Image { get; set; }
}
