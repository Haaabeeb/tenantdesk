using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Moq;
using TenantDesk.Application.Common.Interfaces;
using TenantDesk.Application.Dto.Auth;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Domain.Entities;
using TenantDesk.Domain.Enums;
using TenantDesk.Infrastructure.Identity;
using Xunit;

namespace TenantDesk.UnitTests.Application;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ITenantRepository> _mockTenantRepository;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<IPasswordHasher<User>> _mockPasswordHasher;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly AuthService _sut;
    private readonly string _validJwtToken;

    public AuthServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockTenantRepository = new Mock<ITenantRepository>();
        _mockTokenService = new Mock<ITokenService>();
        _mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _sut = new AuthService(
            _mockUserRepository.Object,
            _mockTenantRepository.Object,
            _mockTokenService.Object,
            _mockPasswordHasher.Object,
            _mockUnitOfWork.Object);

        // Generate a valid JWT token string for GetExpiryFromToken to parse
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("a_very_long_secret_key_for_testing_1234567890"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);
        var tokenHandler = new JwtSecurityTokenHandler();
        _validJwtToken = tokenHandler.WriteToken(token);
    }

    private Tenant CreateValidTenant(Guid tenantId, string subdomain)
    {
        var tenant = new Tenant(Guid.NewGuid(), "Test Tenant", subdomain);
        typeof(Tenant).GetProperty("TenantId")!.SetValue(tenant, tenantId);
        return tenant;
    }

    private User CreateValidUser(Guid userId, Guid tenantId, string email, UserRole role)
    {
        var user = new User(tenantId, email, role);
        user.SetPassword("hashedpassword");
        typeof(User).GetProperty("UserId")!.SetValue(user, userId);
        return user;
    }

    [Fact]
    public async Task RegisterAsync_ValidDto_CreatesUserAndReturnsToken()
    {
        var dto = new RegisterDto("test@example.com", "Password123!", "testsub");
        var tenantId = Guid.NewGuid();
        var tenant = CreateValidTenant(tenantId, dto.TenantSubdomain);

        _mockTenantRepository.Setup(r => r.GetBySubdomainAsync(dto.TenantSubdomain)).ReturnsAsync(tenant);
        _mockUserRepository.Setup(r => r.ExistsByEmailAsync(tenantId, "test@example.com")).ReturnsAsync(false);
        _mockPasswordHasher.Setup(h => h.HashPassword(It.IsAny<User>(), dto.Password)).Returns("hashedpass");
        _mockTokenService.Setup(s => s.TokenGenerateToken(It.IsAny<User>())).Returns(_validJwtToken);

        var result = await _sut.RegisterAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(_validJwtToken, result.Token);
        Assert.Equal("test@example.com", result.Email);
        _mockUserRepository.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == "test@example.com" && u.TenantId == tenantId)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_TenantNotFound_ThrowsKeyNotFoundException()
    {
        var dto = new RegisterDto("test@example.com", "Password123!", "testsub");
        _mockTenantRepository.Setup(r => r.GetBySubdomainAsync(dto.TenantSubdomain)).ReturnsAsync((Tenant?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.RegisterAsync(dto));
    }

    [Fact]
    public async Task RegisterAsync_EmailTaken_ThrowsInvalidOperationException()
    {
        var dto = new RegisterDto("test@example.com", "Password123!", "testsub");
        var tenantId = Guid.NewGuid();
        var tenant = CreateValidTenant(tenantId, dto.TenantSubdomain);

        _mockTenantRepository.Setup(r => r.GetBySubdomainAsync(dto.TenantSubdomain)).ReturnsAsync(tenant);
        _mockUserRepository.Setup(r => r.ExistsByEmailAsync(tenantId, "test@example.com")).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.RegisterAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        var dto = new LoginDto("test@example.com", "Password123!", "testsub");
        var tenantId = Guid.NewGuid();
        var tenant = CreateValidTenant(tenantId, dto.TenantSubdomain);
        var user = CreateValidUser(Guid.NewGuid(), tenantId, "test@example.com", UserRole.Customer);

        _mockTenantRepository.Setup(r => r.GetBySubdomainAsync(dto.TenantSubdomain)).ReturnsAsync(tenant);
        _mockUserRepository.Setup(r => r.GetByEmailAsync("test@example.com", tenantId)).ReturnsAsync(user);
        _mockPasswordHasher.Setup(h => h.VerifyHashedPassword(user, user.PasswordHash, dto.Password)).Returns(PasswordVerificationResult.Success);
        _mockTokenService.Setup(s => s.TokenGenerateToken(user)).Returns(_validJwtToken);

        var result = await _sut.LoginAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(_validJwtToken, result.Token);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task LoginAsync_TenantNotFound_ThrowsUnauthorizedAccessException()
    {
        var dto = new LoginDto("test@example.com", "Password123!", "testsub");
        _mockTenantRepository.Setup(r => r.GetBySubdomainAsync(dto.TenantSubdomain)).ReturnsAsync((Tenant?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.LoginAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        var dto = new LoginDto("test@example.com", "Password123!", "testsub");
        var tenantId = Guid.NewGuid();
        var tenant = CreateValidTenant(tenantId, dto.TenantSubdomain);

        _mockTenantRepository.Setup(r => r.GetBySubdomainAsync(dto.TenantSubdomain)).ReturnsAsync(tenant);
        _mockUserRepository.Setup(r => r.GetByEmailAsync("test@example.com", tenantId)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.LoginAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_UserDeactivated_ThrowsUnauthorizedAccessException()
    {
        var dto = new LoginDto("test@example.com", "Password123!", "testsub");
        var tenantId = Guid.NewGuid();
        var tenant = CreateValidTenant(tenantId, dto.TenantSubdomain);
        var user = CreateValidUser(Guid.NewGuid(), tenantId, "test@example.com", UserRole.Customer);
        user.Deactivate();

        _mockTenantRepository.Setup(r => r.GetBySubdomainAsync(dto.TenantSubdomain)).ReturnsAsync(tenant);
        _mockUserRepository.Setup(r => r.GetByEmailAsync("test@example.com", tenantId)).ReturnsAsync(user);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.LoginAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        var dto = new LoginDto("test@example.com", "WrongPassword!", "testsub");
        var tenantId = Guid.NewGuid();
        var tenant = CreateValidTenant(tenantId, dto.TenantSubdomain);
        var user = CreateValidUser(Guid.NewGuid(), tenantId, "test@example.com", UserRole.Customer);

        _mockTenantRepository.Setup(r => r.GetBySubdomainAsync(dto.TenantSubdomain)).ReturnsAsync(tenant);
        _mockUserRepository.Setup(r => r.GetByEmailAsync("test@example.com", tenantId)).ReturnsAsync(user);
        _mockPasswordHasher.Setup(h => h.VerifyHashedPassword(user, user.PasswordHash, dto.Password)).Returns(PasswordVerificationResult.Failed);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.LoginAsync(dto));
    }
}
