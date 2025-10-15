namespace ERP_API.DTOs;

public record ProductDto(Guid Id, string Sku, string Name, decimal Price, int Stock);
public record ProductCreateDto(string Sku, string Name, decimal Price, int Stock);
public record ProductUpdateDto(string Sku, string Name, decimal Price, int Stock);
