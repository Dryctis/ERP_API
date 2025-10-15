namespace ERP_API.DTOs
{
    public record InventoryMovementCreateDto(
        Guid ProductId,
        int Quantity,          
        int MovementType,
        string? Reason
    );

    public record InventoryMovementDto(
        Guid Id,
        Guid ProductId,
        string ProductName,
        int Quantity,          
        int MovementType,
        string? Reason,
        DateTime CreatedAt
    );
}
