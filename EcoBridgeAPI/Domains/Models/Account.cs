using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EcoBridge.Domains.Models;

public class Account : IdentityUser<int>
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; }

    public Donor? Donor { get; set; }
    public Charity? Charity { get; set; }
    public Volunteer? Volunteer { get; set; }
    public Admin? Admin { get; set; }
}
