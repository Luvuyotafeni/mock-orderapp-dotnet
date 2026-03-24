namespace InventoryService.DTOs;

public class InventoryResponse
{
    public long Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime LastUpdated { get; set; }
}
