using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoBridge.Domains.Models;

public class Donor
{
    [Key]
    [ForeignKey(nameof(Account))]
    public int AccountId { get; set; }

    [Column(TypeName = "tinyint")]
    public byte UserType { get; set; }

    [MaxLength(150)]
    public string? OrganizationName { get; set; }

    public Account Account { get; set; } = null!;
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
}
