using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EcoBridgeAPI.DTO;

public class CreateDonationDTO
{
    [Required]
    [StringLength(100)]
    public string FoodType { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Quantity { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiryDate { get; set; }

    [Required]
    [StringLength(300)]
    public string PickupLocation { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }
}
