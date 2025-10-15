namespace ERP_API.DTOs;

public record LoginRequest(string Email, string Password);
public record TokenResponse(string AccessToken, string RefreshToken);
public record RefreshRequest(string RefreshToken);
public record MeResponse(Guid Id, string Email, string FullName, string[] Roles);
