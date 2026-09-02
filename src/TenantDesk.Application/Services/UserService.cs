using AutoMapper;
using TenantDesk.Application.Common;
using TenantDesk.Application.Dto.User;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Application.Interfaces.Persistence;
using TenantDesk.Domain.Entities;

namespace TenantDesk.Application.Services;

public class UserService(IUserRepository _userRepository, ICurrentTenantService _currentTenantService,IMapper _mapper)
{
    

    
    public async Task<UserResponseDto> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"User with ID '{id}' not found.");

        // tenant scoping: ما بنسمح تشوف مستخدم tenant تاني
        if (user.TenantId != _currentTenantService.GetTenantId())
            throw new UnauthorizedAccessException("You cannot access users from another tenant.");

        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task<PagedResult<UserResponseDto>> GetFilteredAsync(UserFilter filter)
    {
        // tenant scoping: بنفرض TenantId المستخدم الحالي، مش يلي المستخدم بعته بالـ query
        filter.TenantId = _currentTenantService.GetTenantId();

        var result = await _userRepository.GetFilteredAsync(filter);

        return new PagedResult<UserResponseDto>
        {
            Items = result.Items.Select(MapToResponseDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<UserResponseDto> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"User with ID '{id}' not found.");

        if (user.TenantId != _currentTenantService.GetTenantId())
            throw new UnauthorizedAccessException("You cannot modify users from another tenant.");

        var normalizedEmail = dto.Email.Trim().ToLower();

        if (!string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            var emailTaken = await _userRepository.ExistsByEmailAsync(user.TenantId, normalizedEmail);
            if (emailTaken)
                throw new InvalidOperationException("A user with this email already exists in this tenant.");
        }

        user.ChangeEmail(normalizedEmail);
        user.ChangeRole(dto.Role);

        await _userRepository.UpdateAsync(user);

        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task ChangeStatusAsync(Guid id, ChangeUserStatusDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"User with ID '{id}' not found.");

        if (user.TenantId != _currentTenantService.GetTenantId())
            throw new UnauthorizedAccessException("You cannot modify users from another tenant.");

        if (dto.IsActive)
            user.Activate();
        else
            user.Deactivate();

        await _userRepository.UpdateAsync(user);
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"User with ID '{id}' not found.");

        if (user.TenantId != _currentTenantService.GetTenantId())
            throw new UnauthorizedAccessException("You cannot delete users from another tenant.");

        await _userRepository.DeleteAsync(id);
    }

    private static UserResponseDto MapToResponseDto(User user)
    {
        return new UserResponseDto(
            user.UserId,
            user.TenantId,
            user.Email,
            user.Role,
            user.IsActive,
            user.CreatedAt,
            user.UpdatedAt);
    }
}