namespace ERP_API.DTOs
{
    public record ProductStockDto(
        Guid ProductId,
        string Name,
        int Stock
    );
}
