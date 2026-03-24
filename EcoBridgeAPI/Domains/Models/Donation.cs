using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domains.Enums;

namespace EcoBridge.Domains.Models;

public class Donation
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int DonorId { get; set; }

    public int? CharityId { get; set; }
    public int? VolunteerId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FoodType { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Quantity { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime ExpiryDate { get; set; }

    [Required]
    [MaxLength(255)]
    public string PickupLocation { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    [Required]
    [Column(TypeName = "tinyint")]
    public DonationStatus Status { get; set; }

    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    public Donor Donor { get; set; } = null!;
    public Charity? Charity { get; set; }
    public Volunteer? Volunteer { get; set; }
}
