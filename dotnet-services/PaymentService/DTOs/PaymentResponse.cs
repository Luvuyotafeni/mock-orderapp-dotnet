namespace PaymentService.DTOs;

public class PaymentResponse
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string? MaskedCardNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
