namespace PaymentService.DTOs;

public class PaymentResultEvent
{
    public long OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
}
