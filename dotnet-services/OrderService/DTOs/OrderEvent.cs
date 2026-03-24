namespace OrderService.DTOs;

public class OrderEvent
{
    public long OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
