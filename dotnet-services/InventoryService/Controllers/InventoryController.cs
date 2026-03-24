using InventoryService.DTOs;
using InventoryService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateInventory([FromBody] InventoryRequest request)
    {
        var result = await _inventoryService.CreateInventoryAsync(request);
        return StatusCode(201, result);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllInventory()
    {
        var result = await _inventoryService.GetAllInventoryAsync();
        return Ok(result);
    }

    [HttpGet("{productId}")]
    [Authorize]
    public async Task<IActionResult> GetByProductId(string productId)
    {
        var result = await _inventoryService.GetByProductIdAsync(productId);
        return Ok(result);
    }
}
