using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoBridge.Domains.Models;

public class Charity
{
    [Key]
    [ForeignKey(nameof(Account))]
    public int AccountId { get; set; }

    [Required]
    [MaxLength(150)]
    public string OrganizationName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string RegistrationNumber { get; set; } = string.Empty;

    public Account Account { get; set; } = null!;
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
}
