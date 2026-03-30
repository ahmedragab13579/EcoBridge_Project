namespace EcoBridgeAPI.DTO;

public class DonationResponseDTO
{
    public int Id { get; set; }
    public string FoodType { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DonorId { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
