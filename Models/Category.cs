using System.ComponentModel.DataAnnotations;

namespace GearGrid.Models;

public class Category
{
    [Key]
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public ICollection<Equipment> EquipmentItems { get; set; } = new List<Equipment>();
}
