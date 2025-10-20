namespace ERP_API.DTOs;

public record SupplierDto(
    Guid Id,
    string Name,
    string? ContactName,
    string Email,
    string? Phone,
    string? Address,
    string? City,
    string? Country,
    string? TaxId,
    string? Website,
    string? Notes,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int ProductCount 
);


public record SupplierCreateDto(
    string Name,
    string? ContactName,
    string Email,
    string? Phone,
    string? Address,
    string? City,
    string? Country,
    string? TaxId,
    string? Website,
    string? Notes,
    bool IsActive = true
);


public record SupplierUpdateDto(
    string Name,
    string? ContactName,
    string Email,
    string? Phone,
    string? Address,
    string? City,
    string? Country,
    string? TaxId,
    string? Website,
    string? Notes,
    bool IsActive
);


public record SupplierListDto(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? City,
    bool IsActive,
    int ProductCount
);


public record ProductSupplierCreateDto(
    Guid ProductId,
    Guid SupplierId,
    decimal SupplierPrice,
    string? SupplierSku,
    bool IsPreferred = false,
    int? LeadTimeDays = null,
    int? MinimumOrderQuantity = null
);

public record ProductSupplierUpdateDto(
    decimal SupplierPrice,
    string? SupplierSku,
    bool IsPreferred,
    int? LeadTimeDays,
    int? MinimumOrderQuantity
);


public record ProductSupplierDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    Guid SupplierId,
    string SupplierName,
    decimal SupplierPrice,
    string? SupplierSku,
    bool IsPreferred,
    int? LeadTimeDays,
    int? MinimumOrderQuantity,
    DateTime CreatedAt
);


public record SupplierForProductDto(
    Guid SupplierId,
    string SupplierName,
    string Email,
    decimal SupplierPrice,
    string? SupplierSku,
    bool IsPreferred,
    int? LeadTimeDays,
    int? MinimumOrderQuantity
);