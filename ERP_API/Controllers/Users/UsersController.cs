using System.Security.Claims;
using ERP_API.Common.Results;
using ERP_API.DTOs;
using ERP_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_API.Controllers.V1;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service) => _service = service;

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public Task<object> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? q = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? role = null,
        [FromQuery] string? sort = "fullname:asc")
        => _service.GetPagedAsync(page, pageSize, q, isActive, role, sort);

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDetailDto>> Get(Guid id)
    {
        var result = await _service.GetAsync(id);
        return result.ToActionResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(UserCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return result.ToCreatedResult(nameof(Get), new { id = result.Value?.Id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDto>> Update(Guid id, UserUpdateDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result.ToActionResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        return result.ToNoContentResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<UserDto>> Activate(Guid id)
    {
        var result = await _service.ActivateAsync(id);
        return result.ToActionResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/roles")]
    public async Task<IActionResult> AssignRole(Guid id, AssignRoleDto dto)
    {
        var result = await _service.AssignRoleAsync(id, dto);
        return result.ToNoContentResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}/roles/{roleId:guid}")]
    public async Task<IActionResult> RemoveRole(Guid id, Guid roleId)
    {
        var result = await _service.RemoveRoleAsync(id, roleId);
        return result.ToNoContentResult();
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

        if (userIdClaim is null)
            return Unauthorized();

        var userId = Guid.Parse(userIdClaim);
        var result = await _service.ChangePasswordAsync(userId, dto);

        return result.ToNoContentResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, ResetPasswordDto dto)
    {
        var result = await _service.ResetPasswordAsync(id, dto);
        return result.ToNoContentResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("by-role/{roleName}")]
    public async Task<ActionResult<List<UserDto>>> GetByRole(string roleName)
    {
        var result = await _service.GetByRoleAsync(roleName);
        return result.ToActionResult();
    }
}