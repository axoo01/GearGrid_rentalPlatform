using System.ComponentModel.DataAnnotations;

namespace GearGrid.Models;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? NationalIdOrPassport { get; set; }

    [Required]
    public string Role { get; set; } = "Renter";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Company? Company { get; set; }

    public ICollection<RentalOrder> RentalOrders { get; set; } = new List<RentalOrder>();
}
