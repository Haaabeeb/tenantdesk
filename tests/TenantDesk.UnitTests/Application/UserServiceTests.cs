using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using TenantDesk.Application.Common;
using TenantDesk.Application.Dto.User;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Application.Interfaces.Persistence;
using TenantDesk.Application.Interfaces.Persistence.Filters;
using TenantDesk.Application.Services;
using TenantDesk.Domain.Entities;
using TenantDesk.Domain.Enums;
using TenantDesk.UnitTests.Helpers;
using Xunit;

namespace TenantDesk.UnitTests.Application;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ICurrentTenantService> _currentTenantServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UserService _sut;
    private readonly Guid _currentTenantId;

    public UserServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _currentTenantServiceMock = new Mock<ICurrentTenantService>();
        _mapperMock = new Mock<IMapper>();

        _currentTenantId = Guid.NewGuid();
        _currentTenantServiceMock.Setup(c => c.GetTenantId()).Returns(_currentTenantId);

        _sut = new UserService(
            _userRepoMock.Object,
            _currentTenantServiceMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(userId));
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserBelongsToDifferentTenant_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var otherTenantId = Guid.NewGuid();
        var user = new User(otherTenantId, "user@other.com", UserRole.Agent);
        var userId = Guid.NewGuid();
        TestEntityHelper.SetProperty(user, "UserId", userId);

        _userRepoMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.GetByIdAsync(userId));
        Assert.Equal("You cannot access users from another tenant.", ex.Message);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserFoundAndOwned_ReturnsMappedDto()
    {
        // Arrange
        var user = new User(_currentTenantId, "user@current.com", UserRole.Admin);
        var userId = Guid.NewGuid();
        TestEntityHelper.SetProperty(user, "UserId", userId);

        _userRepoMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var expectedDto = new UserResponseDto(userId, _currentTenantId, "user@current.com", UserRole.Admin, true, user.CreatedAt, null);
        _mapperMock.Setup(m => m.Map<UserResponseDto>(user))
            .Returns(expectedDto);

        // Act
        var result = await _sut.GetByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("user@current.com", result.Email);
    }

    [Fact]
    public async Task GetFilteredAsync_EnforcesCurrentTenantIdAndReturnsPagedResult()
    {
        // Arrange
        var filter = new UserFilter { TenantId = Guid.NewGuid(), Page = 1, PageSize = 10 };
        var users = new List<User>
        {
            new User(_currentTenantId, "agent1@tenant.com", UserRole.Agent),
            new User(_currentTenantId, "agent2@tenant.com", UserRole.Agent)
        };

        var pagedUsers = new PagedResult<User>
        {
            Items = users,
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };

        _userRepoMock.Setup(r => r.GetFilteredAsync(It.IsAny<UserFilter>()))
            .ReturnsAsync(pagedUsers);

        // Act
        var result = await _sut.GetFilteredAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(_currentTenantId, filter.TenantId); // TenantId overwritten
    }

    [Fact]
    public async Task UpdateAsync_WhenUserNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new UpdateUserDto("new@email.com", UserRole.Agent);

        _userRepoMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateAsync(userId, dto));
    }

    [Fact]
    public async Task UpdateAsync_WhenUserFromDifferentTenant_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherTenantUser = new User(Guid.NewGuid(), "user@other.com", UserRole.Agent);

        _userRepoMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(otherTenantUser);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.UpdateAsync(userId, new UpdateUserDto("new@email.com", UserRole.Agent)));
        Assert.Equal("You cannot modify users from another tenant.", ex.Message);
        _userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenEmailChangedAndTaken_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User(_currentTenantId, "old@email.com", UserRole.Agent);
        var dto = new UpdateUserDto("taken@email.com", UserRole.Admin);

        _userRepoMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _userRepoMock.Setup(r => r.ExistsByEmailAsync(_currentTenantId, "taken@email.com"))
            .ReturnsAsync(true);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateAsync(userId, dto));
        Assert.Equal("A user with this email already exists in this tenant.", ex.Message);
        _userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenEmailUnchanged_DoesNotCheckEmailUniquenessAndUpdatesRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User(_currentTenantId, "same@email.com", UserRole.Agent);
        var dto = new UpdateUserDto("same@email.com", UserRole.Admin);

        _userRepoMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var expectedDto = new UserResponseDto(userId, _currentTenantId, "same@email.com", UserRole.Admin, true, user.CreatedAt, DateTime.UtcNow);
        _mapperMock.Setup(m => m.Map<UserResponseDto>(user))
            .Returns(expectedDto);

        // Act
        var result = await _sut.UpdateAsync(userId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserRole.Admin, user.Role);
        _userRepoMock.Verify(r => r.ExistsByEmailAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ChangeStatusAsync_WhenUserFoundAndOwned_TogglesActiveState(bool isActive)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User(_currentTenantId, "user@tenant.com", UserRole.Agent);
        if (!isActive)
        {
            Assert.True(user.IsActive);
        }
        else
        {
            user.Deactivate();
            Assert.False(user.IsActive);
        }

        _userRepoMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        await _sut.ChangeStatusAsync(userId, new ChangeUserStatusDto(isActive));

        // Assert
        Assert.Equal(isActive, user.IsActive);
        _userRepoMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(userId));
    }

    [Fact]
    public async Task DeleteAsync_WhenUserFromDifferentTenant_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUser = new User(Guid.NewGuid(), "other@mail.com", UserRole.Agent);

        _userRepoMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(otherUser);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.DeleteAsync(userId));
        Assert.Equal("You cannot delete users from another tenant.", ex.Message);
        _userRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserFoundAndOwned_DeletesFromRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User(_currentTenantId, "user@tenant.com", UserRole.Agent);

        _userRepoMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        await _sut.DeleteAsync(userId);

        // Assert
        _userRepoMock.Verify(r => r.DeleteAsync(userId), Times.Once);
    }
}
