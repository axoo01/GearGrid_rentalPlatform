using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GearGrid.Models;

public class RentalOrder
{
    [Key]
    public int OrderId { get; set; }

    [Required]
    [MaxLength(30)]
    public string OrderReference { get; set; } = string.Empty;

    public int RenterUserId { get; set; }

    public int CompanyId { get; set; }

    public int EquipmentId { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public int TotalDays { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; }

    [Required]
    public string OrderStatus { get; set; } = "Pending";

    public string? ProviderNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(RenterUserId))]
    public User Renter { get; set; } = null!;

    [ForeignKey(nameof(CompanyId))]
    public Company Company { get; set; } = null!;

    [ForeignKey(nameof(EquipmentId))]
    public Equipment Equipment { get; set; } = null!;

    public RentalAgreement? RentalAgreement { get; set; }
}
