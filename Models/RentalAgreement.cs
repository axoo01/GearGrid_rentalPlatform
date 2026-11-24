using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GearGrid.Models;

public class RentalAgreement
{
    [Key]
    public int AgreementId { get; set; }

    public int OrderId { get; set; }

    [Required]
    public string ContractSnapshotText { get; set; } = string.Empty;

    [Required]
    public bool RenterAccepted { get; set; }

    [Required]
    public DateTime AcceptedAt { get; set; }

    [MaxLength(200)]
    public string? RenterIpOrDeviceNote { get; set; }

    [ForeignKey(nameof(OrderId))]
    public RentalOrder RentalOrder { get; set; } = null!;
}
