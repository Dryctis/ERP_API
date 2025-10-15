using System;

namespace ERP_API.Entities
{
    public class InventoryMovement
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }  
        public MovementType MovementType { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relación
        public Product Product { get; set; } = default!;
    }
}
