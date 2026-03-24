using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoBridge.Domains.Models;

public class Admin
{
    [Key]
    [ForeignKey(nameof(Account))]
    public int AccountId { get; set; }

    public Account Account { get; set; } = null!;
}
