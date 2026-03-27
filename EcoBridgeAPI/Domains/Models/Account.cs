using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domains.Enums;

namespace EcoBridge.Domains.Models;

public class Account
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "tinyint")]
    public UserRole RoleId { get; set; }

    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    public Donor? Donor { get; set; }
    public Charity? Charity { get; set; }
    public Volunteer? Volunteer { get; set; }
    public Admin? Admin { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
