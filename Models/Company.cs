using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GearGrid.Models;

public class Company
{
    [Key]
    public int CompanyId { get; set; }

    public int OwnerUserId { get; set; }

    [Required]
    [MaxLength(150)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string BusinessPhone { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string BusinessEmail { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(250)]
    public string PhysicalAddress { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    [Required]
    public string DefaultTermsAndConditions { get; set; } = string.Empty;

    [Required]
    public string VerificationStatus { get; set; } = "Pending";

    public string? AdminNotes { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public int? VerifiedByAdminId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(OwnerUserId))]
    public User Owner { get; set; } = null!;

    [ForeignKey(nameof(VerifiedByAdminId))]
    public User? VerifiedByAdmin { get; set; }

    public ICollection<Equipment> EquipmentItems { get; set; } = new List<Equipment>();

    public ICollection<RentalOrder> RentalOrders { get; set; } = new List<RentalOrder>();
}
