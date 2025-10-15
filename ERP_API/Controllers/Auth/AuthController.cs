using System.Security.Claims;
using ERP_API.DTOs;
using ERP_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_API.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _svc;
    public AuthController(IAuthService svc) => _svc = svc;

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequest req)
        => (await _svc.LoginAsync(req.Email, req.Password)) is { } t ? Ok(t) : Unauthorized();

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponse>> Refresh(RefreshRequest req)
        => (await _svc.RefreshAsync(req.RefreshToken)) is { } t ? Ok(t) : Unauthorized();

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequest req)
    {
        await _svc.RevokeAsync(req.RefreshToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<MeResponse>> Me()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (sub is null) return Unauthorized();
        var me = await _svc.MeAsync(Guid.Parse(sub));
        return me is null ? NotFound() : Ok(me);
    }
}
