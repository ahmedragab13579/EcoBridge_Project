using Microsoft.AspNetCore.Http;

namespace EcoBridgeAPI.DTO;

public class UpdateDonationDTO
{
    public string? FoodType { get; set; }
    public string? Quantity { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? PickupLocation { get; set; }
    public IFormFile? Image { get; set; }
}
