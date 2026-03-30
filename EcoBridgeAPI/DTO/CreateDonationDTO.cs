using Microsoft.AspNetCore.Http;

namespace EcoBridgeAPI.DTO;

public class CreateDonationDTO
{
    public string FoodType { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public IFormFile? Image { get; set; }
}
