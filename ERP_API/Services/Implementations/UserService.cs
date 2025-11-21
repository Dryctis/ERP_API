using ERP_API.Common.Constants;
using ERP_API.Common.Results;
using ERP_API.DTOs;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using ERP_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUnidadDeTrabajo _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(IUnidadDeTrabajo unitOfWork, ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<object> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        bool? isActive = null,
        string? role = null,
        string? sort = null)
    {
        _logger.LogDebug(
            "Consultando usuarios paginados. Página: {Page}, Tamaño: {PageSize}",
            page, pageSize
        );

        var (items, total) = await _unitOfWork.Users.GetPagedAsync(
            page, pageSize, searchTerm, isActive, role, sort);

        var result = items.Select(u => new UserListDto(
            u.Id,
            u.Email,
            u.FullName,
            u.IsActive,
            u.UserRoles.Count
        )).ToList();

        _logger.LogInformation(
            "Usuarios obtenidos. Total: {Total}, Página: {Page}, Resultados: {Count}",
            total, page, result.Count
        );

        return new { total, page, pageSize, items = result };
    }

    public async Task<Result<UserDetailDto>> GetAsync(Guid id)
    {
        _logger.LogDebug("Consultando usuario. UserId: {UserId}", id);

        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(id);

        if (user is null)
        {
            _logger.LogWarning("Usuario no encontrado. UserId: {UserId}", id);
            return Result<UserDetailDto>.Failure("User not found");
        }

        var dto = new UserDetailDto(
            user.Id,
            user.Email,
            user.FullName,
            user.IsActive,
            DateTime.UtcNow,
            user.UserRoles.Select(ur => new UserRoleDto(
                ur.RoleId,
                ur.Role.Name,
                DateTime.UtcNow
            )).ToList()
        );

        return Result<UserDetailDto>.Success(dto);
    }

    public async Task<Result<UserDto>> CreateAsync(UserCreateDto dto)
    {
        _logger.LogInformation(
            "Creando usuario. Email: {Email}, FullName: {FullName}",
            dto.Email, dto.FullName
        );

        if (await _unitOfWork.Users.ExistsByEmailAsync(dto.Email))
        {
            _logger.LogWarning("Email ya existe. Email: {Email}", dto.Email);
            return Result<UserDto>.Failure("Email already exists");
        }

        var user = new User
        {
            Email = dto.Email,
            FullName = dto.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsActive = dto.IsActive
        };

        var roles = dto.Roles ?? new List<string> { RoleConstants.User };
        var roleEntities = await _unitOfWork.GetDbContext().Roles
            .Where(r => roles.Contains(r.Name))
            .ToListAsync();

        if (roleEntities.Count != roles.Count)
        {
            return Result<UserDto>.Failure("One or more roles are invalid");
        }

        foreach (var role in roleEntities)
        {
            user.UserRoles.Add(new UserRole
            {
                User = user,
                Role = role
            });
        }

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Usuario creado. UserId: {UserId}, Email: {Email}",
            user.Id, user.Email
        );

        var result = new UserDto(
            user.Id,
            user.Email,
            user.FullName,
            user.IsActive,
            DateTime.UtcNow,
            roles
        );

        return Result<UserDto>.Success(result);
    }

    public async Task<Result<UserDto>> UpdateAsync(Guid id, UserUpdateDto dto)
    {
        _logger.LogInformation("Actualizando usuario. UserId: {UserId}", id);

        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(id);

        if (user is null)
        {
            _logger.LogWarning("Usuario no encontrado. UserId: {UserId}", id);
            return Result<UserDto>.Failure("User not found");
        }

        if (await _unitOfWork.Users.ExistsByEmailAsync(dto.Email, id))
        {
            _logger.LogWarning("Email ya existe. Email: {Email}", dto.Email);
            return Result<UserDto>.Failure("Email already exists");
        }

        user.Email = dto.Email;
        user.FullName = dto.FullName;
        user.IsActive = dto.IsActive;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Usuario actualizado. UserId: {UserId}", id);

        var result = new UserDto(
            user.Id,
            user.Email,
            user.FullName,
            user.IsActive,
            DateTime.UtcNow,
            user.UserRoles.Select(ur => ur.Role.Name).ToList()
        );

        return Result<UserDto>.Success(result);
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Desactivando usuario. UserId: {UserId}", id);

        var user = await _unitOfWork.Users.GetByIdAsync(id);

        if (user is null)
        {
            _logger.LogWarning("Usuario no encontrado. UserId: {UserId}", id);
            return Result.Failure("User not found");
        }

        user.IsActive = false;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogWarning("Usuario desactivado. UserId: {UserId}", id);

        return Result.Success();
    }

    public async Task<Result<UserDto>> ActivateAsync(Guid id)
    {
        _logger.LogInformation("Activando usuario. UserId: {UserId}", id);

        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(id);

        if (user is null)
        {
            _logger.LogWarning("Usuario no encontrado. UserId: {UserId}", id);
            return Result<UserDto>.Failure("User not found");
        }

        user.IsActive = true;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Usuario activado. UserId: {UserId}", id);

        var result = new UserDto(
            user.Id,
            user.Email,
            user.FullName,
            user.IsActive,
            DateTime.UtcNow,
            user.UserRoles.Select(ur => ur.Role.Name).ToList()
        );

        return Result<UserDto>.Success(result);
    }

    public async Task<Result> AssignRoleAsync(Guid userId, AssignRoleDto dto)
    {
        _logger.LogInformation(
            "Asignando rol. UserId: {UserId}, Role: {Role}",
            userId, dto.RoleName
        );

        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(userId);

        if (user is null)
        {
            _logger.LogWarning("Usuario no encontrado. UserId: {UserId}", userId);
            return Result.Failure("User not found");
        }

        if (!RoleConstants.IsValidRole(dto.RoleName))
        {
            _logger.LogWarning("Rol inválido. Role: {Role}", dto.RoleName);
            return Result.Failure("Invalid role");
        }

        if (user.UserRoles.Any(ur => ur.Role.Name == dto.RoleName))
        {
            _logger.LogWarning(
                "Usuario ya tiene el rol. UserId: {UserId}, Role: {Role}",
                userId, dto.RoleName
            );
            return Result.Failure("User already has this role");
        }

        var role = await _unitOfWork.GetDbContext().Roles
            .FirstOrDefaultAsync(r => r.Name == dto.RoleName);

        if (role is null)
        {
            return Result.Failure("Role not found");
        }

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = role.Id
        };

        _unitOfWork.GetDbContext().UserRoles.Add(userRole);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Rol asignado. UserId: {UserId}, Role: {Role}",
            userId, dto.RoleName
        );

        return Result.Success();
    }

    public async Task<Result> RemoveRoleAsync(Guid userId, Guid roleId)
    {
        _logger.LogInformation(
            "Removiendo rol. UserId: {UserId}, RoleId: {RoleId}",
            userId, roleId
        );

        var userRole = await _unitOfWork.GetDbContext().UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (userRole is null)
        {
            _logger.LogWarning(
                "Relación usuario-rol no encontrada. UserId: {UserId}, RoleId: {RoleId}",
                userId, roleId
            );
            return Result.Failure("User role not found");
        }

        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(userId);

        if (user!.UserRoles.Count == 1)
        {
            _logger.LogWarning(
                "No se puede remover el último rol. UserId: {UserId}",
                userId
            );
            return Result.Failure("Cannot remove the last role from user");
        }

        _unitOfWork.GetDbContext().UserRoles.Remove(userRole);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Rol removido. UserId: {UserId}, RoleId: {RoleId}",
            userId, roleId
        );

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        _logger.LogInformation("Cambiando contraseña. UserId: {UserId}", userId);

        var user = await _unitOfWork.Users.GetByIdAsync(userId);

        if (user is null)
        {
            _logger.LogWarning("Usuario no encontrado. UserId: {UserId}", userId);
            return Result.Failure("User not found");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            _logger.LogWarning("Contraseña actual incorrecta. UserId: {UserId}", userId);
            return Result.Failure("Current password is incorrect");
        }

        if (dto.NewPassword != dto.ConfirmPassword)
        {
            return Result.Failure("New password and confirmation do not match");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Contraseña cambiada. UserId: {UserId}", userId);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(Guid userId, ResetPasswordDto dto)
    {
        _logger.LogInformation("Reseteando contraseña. UserId: {UserId}", userId);

        var user = await _unitOfWork.Users.GetByIdAsync(userId);

        if (user is null)
        {
            _logger.LogWarning("Usuario no encontrado. UserId: {UserId}", userId);
            return Result.Failure("User not found");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogWarning("Contraseña reseteada por admin. UserId: {UserId}", userId);

        return Result.Success();
    }

    public async Task<Result<List<UserDto>>> GetByRoleAsync(string roleName)
    {
        _logger.LogDebug("Consultando usuarios por rol. Role: {Role}", roleName);

        if (!RoleConstants.IsValidRole(roleName))
        {
            return Result<List<UserDto>>.Failure("Invalid role");
        }

        var users = await _unitOfWork.Users.GetByRoleAsync(roleName);

        var result = users.Select(u => new UserDto(
            u.Id,
            u.Email,
            u.FullName,
            u.IsActive,
            DateTime.UtcNow,
            u.UserRoles.Select(ur => ur.Role.Name).ToList()
        )).ToList();

        return Result<List<UserDto>>.Success(result);
    }
}