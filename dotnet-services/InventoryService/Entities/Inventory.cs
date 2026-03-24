using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryService.Entities;

[Table("inventory")]
public class Inventory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public string ProductId { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
