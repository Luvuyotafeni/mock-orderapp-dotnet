namespace InventoryService.DTOs;

public class InventoryRequest
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
