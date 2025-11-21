using ERP_API.Common.Results;
using ERP_API.DTOs;

namespace ERP_API.Services.Interfaces;

public interface IUserService
{
    Task<object> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        bool? isActive = null,
        string? role = null,
        string? sort = null);

    Task<Result<UserDetailDto>> GetAsync(Guid id);
    Task<Result<UserDto>> CreateAsync(UserCreateDto dto);
    Task<Result<UserDto>> UpdateAsync(Guid id, UserUpdateDto dto);
    Task<Result> DeleteAsync(Guid id);
    Task<Result<UserDto>> ActivateAsync(Guid id);
    Task<Result> AssignRoleAsync(Guid userId, AssignRoleDto dto);
    Task<Result> RemoveRoleAsync(Guid userId, Guid roleId);
    Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    Task<Result> ResetPasswordAsync(Guid userId, ResetPasswordDto dto);
    Task<Result<List<UserDto>>> GetByRoleAsync(string roleName);
}