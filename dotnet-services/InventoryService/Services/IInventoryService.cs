using InventoryService.DTOs;

namespace InventoryService.Services;

public interface IInventoryService
{
    Task<InventoryResponse> CreateInventoryAsync(InventoryRequest request);
    Task<List<InventoryResponse>> GetAllInventoryAsync();
    Task<InventoryResponse> GetByProductIdAsync(string productId);
    Task DeductInventoryAsync(string productId, int quantity);
}
