using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using ERP_API.Data;
using ERP_API.DTOs;
using ERP_API.Entities;
using ERP_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ERP_API.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _cfg;

    public AuthService(AppDbContext db, IConfiguration cfg)
    {
        _db = db; _cfg = cfg;
    }

    public async Task<TokenResponse?> LoginAsync(string email, string password)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        var (access, refresh) = await IssueTokensAsync(user);
        return new TokenResponse(access, refresh);
    }

    public async Task<TokenResponse?> RefreshAsync(string refreshToken)
    {
        var token = await _db.RefreshTokens
            .Include(t => t.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.Revoked);

        if (token is null || token.ExpiresAt < DateTime.UtcNow) return null;

        token.Revoked = true; 
        var (access, newRefresh) = await IssueTokensAsync(token.User, token.Token);
        await _db.SaveChangesAsync();

        return new TokenResponse(access, newRefresh);
    }

    public async Task RevokeAsync(string refreshToken)
    {
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
        if (token is null) return;
        token.Revoked = true;
        await _db.SaveChangesAsync();
    }

    public async Task<MeResponse?> MeAsync(Guid userId)
    {
        var u = await _db.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId);
        if (u is null) return null;
        var roles = u.UserRoles.Select(r => r.Role.Name).ToArray();
        return new MeResponse(u.Id, u.Email, u.FullName, roles);
    }

    private async Task<(string access, string refresh)> IssueTokensAsync(User user, string? replacedBy = null)
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
        _db.RefreshTokens.Add(refresh);
        await _db.SaveChangesAsync();

        return (jwt, refresh.Token);
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

        var token = new JwtSecurityToken(issuer, audience, claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
