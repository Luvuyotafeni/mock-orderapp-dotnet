using InventoryService.Data;
using InventoryService.DTOs;
using InventoryService.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Services.Impl;

public class InventoryServiceImpl : IInventoryService
{
    private readonly AppDbContext _db;
    private readonly ILogger<InventoryServiceImpl> _logger;

    public InventoryServiceImpl(AppDbContext db, ILogger<InventoryServiceImpl> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<InventoryResponse> CreateInventoryAsync(InventoryRequest request)
    {
        if (await _db.Inventories.AnyAsync(i => i.ProductId == request.ProductId))
            throw new Exception($"Product already exists: {request.ProductId}");

        var inventory = new Inventory
        {
            ProductId = request.ProductId,
            Quantity = request.Quantity
        };

        _db.Inventories.Add(inventory);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created inventory for {ProductId} with qty {Quantity}", request.ProductId, request.Quantity);
        return ToResponse(inventory);
    }

    public async Task<List<InventoryResponse>> GetAllInventoryAsync()
    {
        var items = await _db.Inventories.ToListAsync();
        return items.Select(ToResponse).ToList();
    }

    public async Task<InventoryResponse> GetByProductIdAsync(string productId)
    {
        var item = await _db.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId)
            ?? throw new Exception($"Product not found: {productId}");
        return ToResponse(item);
    }

    public async Task DeductInventoryAsync(string productId, int quantity)
    {
        var item = await _db.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId)
            ?? new Inventory { ProductId = productId, Quantity = 100 };

        if (item.Id == 0) _db.Inventories.Add(item);

        if (item.Quantity >= quantity)
        {
            item.Quantity -= quantity;
            item.LastUpdated = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Deducted {Qty} for {ProductId}. Remaining: {Remaining}", quantity, productId, item.Quantity);
        }
        else
        {
            _logger.LogWarning("Insufficient stock for {ProductId}. Requested: {Qty}, Available: {Available}", productId, quantity, item.Quantity);
        }
    }

    private static InventoryResponse ToResponse(Inventory i) => new()
    {
        Id = i.Id,
        ProductId = i.ProductId,
        Quantity = i.Quantity,
        LastUpdated = i.LastUpdated
    };
}
