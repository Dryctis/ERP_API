using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ERP_API.DTOs;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using ERP_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ERP_API.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUnidadDeTrabajo _unitOfWork;
    private readonly IConfiguration _cfg;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUnidadDeTrabajo unitOfWork, IConfiguration cfg, ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _cfg = cfg;
        _logger = logger;
    }

    public async Task<TokenResponse?> LoginAsync(string email, string password)
    {
        _logger.LogInformation("Intento de login. Email: {Email}", email);

        var user = await _unitOfWork.GetDbContext().Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        if (user is null)
        {
            _logger.LogWarning("Login fallido: Usuario no encontrado o inactivo. Email: {Email}", email);
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            _logger.LogWarning("Login fallido: Contraseña incorrecta. Email: {Email}, UserId: {UserId}", email, user.Id);
            return null;
        }

        var roles = user.UserRoles.Select(r => r.Role.Name).ToArray();

        var (access, refresh) = await IssueTokensAsync(user);

        _logger.LogInformation(
            "Login exitoso. UserId: {UserId}, Email: {Email}, Roles: {Roles}",
            user.Id,
            user.Email,
            string.Join(", ", roles)
        );

        return new TokenResponse(access, refresh);
    }

    public async Task<TokenResponse?> RefreshAsync(string refreshToken)
    {
        _logger.LogDebug("Intento de refresh token");

        var token = await _unitOfWork.GetDbContext().RefreshTokens
            .Include(t => t.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.Revoked);

        if (token is null || token.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token inválido o expirado. Token: {Token}", refreshToken?.Substring(0, 10) + "...");
            return null;
        }

        token.Revoked = true;
        var (access, newRefresh) = await IssueTokensAsync(token.User, token.Token);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Token refrescado exitosamente. UserId: {UserId}, Email: {Email}",
            token.User.Id,
            token.User.Email
        );

        return new TokenResponse(access, newRefresh);
    }

    public async Task RevokeAsync(string refreshToken)
    {
        _logger.LogInformation("Revocando refresh token");

        var token = await _unitOfWork.GetDbContext().RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (token is null)
        {
            _logger.LogWarning("Intento de revocar token inexistente");
            return;
        }

        token.Revoked = true;
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Refresh token revocado. UserId: {UserId}, Token: {Token}",
            token.UserId,
            refreshToken.Substring(0, 10) + "..."
        );
    }

    public async Task<MeResponse?> MeAsync(Guid userId)
    {
        _logger.LogDebug("Consultando información de usuario. UserId: {UserId}", userId);

        var user = await _unitOfWork.GetDbContext().Users
            .Include(x => x.UserRoles).ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user is null)
        {
            _logger.LogWarning("Usuario no encontrado. UserId: {UserId}", userId);
            return null;
        }

        var roles = user.UserRoles.Select(r => r.Role.Name).ToArray();

        _logger.LogDebug(
            "Información de usuario obtenida. UserId: {UserId}, Email: {Email}, Roles: {Roles}",
            user.Id,
            user.Email,
            string.Join(", ", roles)
        );

        return new MeResponse(user.Id, user.Email, user.FullName, roles);
    }

 
    private Task<(string access, string refresh)> IssueTokensAsync(User user, string? replacedBy = null)
    {
        var roles = user.UserRoles.Select(r => r.Role.Name).ToArray();

        var jwt = CreateJwt(user, roles);
        var refresh = new RefreshToken
        {
            UserId = user.Id,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(_cfg.GetValue("Jwt:RefreshDays", 7)),
            ReplacedByToken = replacedBy
        };

        _unitOfWork.GetDbContext().RefreshTokens.Add(refresh);

        _logger.LogDebug(
            "Tokens generados. UserId: {UserId}, RefreshToken expira: {ExpiresAt}",
            user.Id,
            refresh.ExpiresAt
        );

        return Task.FromResult((jwt, refresh.Token));
    }


    private string CreateJwt(User user, string[] roles)
    {
        var issuer = _cfg["Jwt:Issuer"]!;
        var audience = _cfg["Jwt:Audience"]!;
        var secret = _cfg["Jwt:Secret"]!;
        var minutes = _cfg.GetValue("Jwt:AccessMinutes", 30);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.FullName)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
