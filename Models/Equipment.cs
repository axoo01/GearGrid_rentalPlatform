using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GearGrid.Models;

public class Equipment
{
    [Key]
    public int EquipmentId { get; set; }

    public int CompanyId { get; set; }

    public int CategoryId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string SerialOrAssetCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal DailyRate { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal SecurityDeposit { get; set; }

    [Required]
    public int TotalQuantity { get; set; }

    [Required]
    public int AvailableQuantity { get; set; }

    [Required]
    public string Status { get; set; } = "Available";

    public string? SpecificTerms { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company Company { get; set; } = null!;

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;

    public ICollection<RentalOrder> RentalOrders { get; set; } = new List<RentalOrder>();
}
