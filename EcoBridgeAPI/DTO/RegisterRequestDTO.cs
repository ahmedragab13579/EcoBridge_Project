using EcoBridge.Domains.Enums;

namespace EcoBridgeAPI.DTO;

public class RegisterRequestDTO
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole RoleId { get; set; } = UserRole.Donor;
}
