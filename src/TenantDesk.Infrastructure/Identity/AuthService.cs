using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Application.Common.Interfaces;
using TenantDesk.Application.Dto.Auth;
using TenantDesk.Domain.Entities;
using TenantDesk.Domain.Enums;

namespace TenantDesk.Infrastructure.Identity;

public class AuthService(
    IUserRepository userRepository,
    ITenantRepository _tenantRepository,
    ITokenService _tokenService,
    IPasswordHasher<User> _passwordHasher,
    IUnitOfWork _unitOfWork) : IAuthService
{
    

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // 1. Resolve the tenant from the subdomain the frontend sent.
        //    If it doesn't exist, we can't know which tenant to attach
        //    this user to, so we stop here.
        var tenant = await _tenantRepository.GetBySubdomainAsync(dto.TenantSubdomain)
            ?? throw new KeyNotFoundException("Tenant not found.");

        // 2. Prevent duplicate accounts WITHIN this tenant. Per-tenant
        //    uniqueness means the same email CAN exist in different tenants,
        //    but not twice in the same one — that's exactly what
        //    ExistsByEmailAsync(tenantId, email) checks.
        var emailTaken = await userRepository.ExistsByEmailAsync(tenant.TenantId, dto.Email.Trim().ToLower());
        if (emailTaken)
            throw new InvalidOperationException("A user with this email already exists in this tenant.");

        // 3. Create the user WITHOUT a password (see Step 2 change to User.cs),
        //    then hash the real password and attach it explicitly.
        //    Default role here is Customer — change this if your business
        //    rule is different (e.g. invite-only admins vs self-signup customers).
        var user = new User(tenant.TenantId, dto.Email, UserRole.Customer);
        var hash = _passwordHasher.HashPassword(user, dto.Password);
        user.SetPassword(hash);

        // 4. Persist to the database. AddAsync stages the change in the
        //    DbContext; the actual SQL INSERT + commit happens in
        //    _unitOfWork.SaveChangesAsync() — this is the pattern your
        //    UnitOfWork class already uses everywhere else in the project.
        await userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // 5. Issue a token immediately so the user is logged in right after
        //    registering (no separate login step required).
        var token = _tokenService.TokenGenerateToken(user);
        var expiresAt = GetExpiryFromToken(token);
        return new AuthResponseDto(token, user.UserId, user.TenantId, user.Email, user.Role, expiresAt);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        // Resolve tenant the same way as registration.
        var tenant = await _tenantRepository.GetBySubdomainAsync(dto.TenantSubdomain)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        // Tenant-scoped lookup: finds the user only if they belong to THIS
        // tenant, so there's no ambiguity if the same email exists under a
        // different tenant.
        var user = await userRepository.GetByEmailAsync(dto.Email.Trim().ToLower(), tenant.TenantId)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        // Note: we deliberately throw the SAME generic "Invalid credentials"
        // message whether the tenant, email, or password is wrong. This
        // stops attackers from figuring out (e.g.) "this email exists but
        // the password is wrong" — a common security leak in login endpoints.

        if (!user.IsActive)
            throw new UnauthorizedAccessException("This account has been deactivated.");

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var token = _tokenService.TokenGenerateToken(user);
        var expiresAt = GetExpiryFromToken(token);
        return new AuthResponseDto(token, user.UserId, user.TenantId, user.Email, user.Role, expiresAt);
    }

    // ITokenService only returns the raw JWT string, not an expiry value
    // separately. Rather than recompute/duplicate the expiry duration here
    // (which could drift out of sync with whatever TokenService actually
    // set inside the token), we read it straight out of the token's own
    // "exp" claim. The token itself stays the single source of truth.
    private static DateTime GetExpiryFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return jwt.ValidTo; 
    }
}