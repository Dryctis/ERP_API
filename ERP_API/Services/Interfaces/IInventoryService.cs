using ERP_API.DTOs;

namespace ERP_API.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryMovementDto>> GetAllAsync();
        Task<InventoryMovementDto?> GetByIdAsync(Guid id);
        Task<InventoryMovementDto> CreateAsync(InventoryMovementCreateDto dto);

        Task<ProductStockDto?> GetStockAsync(Guid productId);                
        Task<IEnumerable<InventoryMovementDto>> GetFilteredAsync(Guid? productId, DateTime? from, DateTime? to); 
        Task<IEnumerable<ProductStockDto>> GetLowStockAsync(int threshold);   
    }
}
