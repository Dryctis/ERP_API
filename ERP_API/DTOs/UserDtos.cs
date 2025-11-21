namespace ERP_API.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string FullName,
    bool IsActive,
    DateTime CreatedAt,
    List<string> Roles
);

public record UserListDto(
    Guid Id,
    string Email,
    string FullName,
    bool IsActive,
    int RoleCount
);

public record UserCreateDto(
    string Email,
    string FullName,
    string Password,
    bool IsActive = true,
    List<string>? Roles = null
);

public record UserUpdateDto(
    string Email,
    string FullName,
    bool IsActive
);

public record ChangePasswordDto(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
);

public record ResetPasswordDto(
    string NewPassword
);

public record AssignRoleDto(
    string RoleName
);

public record UserDetailDto(
    Guid Id,
    string Email,
    string FullName,
    bool IsActive,
    DateTime CreatedAt,
    List<UserRoleDto> Roles
);

public record UserRoleDto(
    Guid RoleId,
    string RoleName,
    DateTime AssignedAt
);