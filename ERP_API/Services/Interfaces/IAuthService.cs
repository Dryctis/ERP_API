using ERP_API.DTOs;

namespace ERP_API.Services.Interfaces;

public interface IAuthService
{
    Task<TokenResponse?> LoginAsync(string email, string password);
    Task<TokenResponse?> RefreshAsync(string refreshToken);
    Task RevokeAsync(string refreshToken);
    Task<MeResponse?> MeAsync(Guid userId);
}
