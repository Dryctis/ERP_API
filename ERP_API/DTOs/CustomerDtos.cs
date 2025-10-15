namespace ERP_API.DTOs;

public record CustomerDto(Guid Id, string Name, string Email, string? Phone, DateTime CreatedAt);
public record CustomerCreateDto(string Name, string Email, string? Phone);
public record CustomerUpdateDto(string Name, string Email, string? Phone);
