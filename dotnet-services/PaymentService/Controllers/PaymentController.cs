using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.DTOs;
using PaymentService.Services;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("pay/{orderId}")]
    public async Task<IActionResult> Pay(long orderId, [FromBody] PaymentRequest request)
    {
        var result = await _paymentService.PayAsync(orderId, request);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPayments()
    {
        var result = await _paymentService.GetAllPaymentsAsync();
        return Ok(result);
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetPaymentByOrderId(long orderId)
    {
        var result = await _paymentService.GetPaymentByOrderIdAsync(orderId);
        return Ok(result);
    }
}
