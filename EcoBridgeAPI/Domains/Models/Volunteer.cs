using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoBridge.Domains.Models;

public class Volunteer
{
    [Key]
    [ForeignKey(nameof(Account))]
    public int AccountId { get; set; }

    [MaxLength(100)]
    public string? VehicleDetails { get; set; }

    public Account Account { get; set; } = null!;
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
}
