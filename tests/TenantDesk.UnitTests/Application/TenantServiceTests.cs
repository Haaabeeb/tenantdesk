using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using TenantDesk.Application.Dto.Tenant;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Services;
using TenantDesk.Domain.Entities;
using TenantDesk.UnitTests.Helpers;
using Xunit;

namespace TenantDesk.UnitTests.Application;

public class TenantServiceTests
{
    private readonly Mock<ITenantRepository> _tenantRepoMock;
    private readonly Mock<IPlanRepository> _planRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly TenantService _sut;

    public TenantServiceTests()
    {
        _tenantRepoMock = new Mock<ITenantRepository>();
        _planRepoMock = new Mock<IPlanRepository>();
        _mapperMock = new Mock<IMapper>();

        _sut = new TenantService(
            _tenantRepoMock.Object,
            _planRepoMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenPlanDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var dto = new CreateTenantDto(planId, "New Corp", "newcorp");

        _planRepoMock.Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync((Plan?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CreateAsync(dto));
        Assert.Equal("Plan not found.", ex.Message);
        _tenantRepoMock.Verify(r => r.AddAsync(It.IsAny<Tenant>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenSubdomainAlreadyInUse_ThrowsInvalidOperationException()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var plan = new Plan("Gold", 100);
        var dto = new CreateTenantDto(planId, "New Corp", "taken-subdomain");

        _planRepoMock.Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync(plan);

        _tenantRepoMock.Setup(r => r.ExistsBySubdomainAsync("taken-subdomain"))
            .ReturnsAsync(true);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(dto));
        Assert.Equal("Subdomain is already in use.", ex.Message);
        _tenantRepoMock.Verify(r => r.AddAsync(It.IsAny<Tenant>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesTenantAndReturnsDto()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var plan = new Plan("Pro", 200);
        var dto = new CreateTenantDto(planId, "Acme Corp", "acme-tenant");

        _planRepoMock.Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync(plan);

        _tenantRepoMock.Setup(r => r.ExistsBySubdomainAsync("acme-tenant"))
            .ReturnsAsync(false);

        var expectedDto = new TenantResponseDto(Guid.NewGuid(), planId, dto.TenantName, dto.Subdomain, true, DateTime.UtcNow, null);

        _mapperMock.Setup(m => m.Map<TenantResponseDto>(It.IsAny<Tenant>()))
            .Returns(expectedDto);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.TenantName, result.TenantName);
        _tenantRepoMock.Verify(r => r.AddAsync(It.Is<Tenant>(t => t.TenantName == dto.TenantName && t.Subdomain == dto.Subdomain)), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTenantNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId))
            .ReturnsAsync((Tenant?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(tenantId));
    }

    [Fact]
    public async Task GetByIdAsync_WhenTenantFound_ReturnsMappedDto()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = new Tenant(Guid.NewGuid(), "Acme", "acme");
        TestEntityHelper.SetProperty(tenant, "TenantId", tenantId);

        var expectedDto = new TenantResponseDto(tenantId, tenant.PlanId, "Acme", "acme", true, tenant.CreatedAt, null);

        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId))
            .ReturnsAsync(tenant);

        _mapperMock.Setup(m => m.Map<TenantResponseDto>(tenant))
            .Returns(expectedDto);

        // Act
        var result = await _sut.GetByIdAsync(tenantId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tenantId, result.TenantId);
        Assert.Equal("Acme", result.TenantName);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedTenantList()
    {
        // Arrange
        var tenants = new List<Tenant>
        {
            new Tenant(Guid.NewGuid(), "Tenant 1", "t1"),
            new Tenant(Guid.NewGuid(), "Tenant 2", "t2")
        };

        var dtos = tenants.Select(t => new TenantListDto(Guid.NewGuid(), t.TenantName, t.Subdomain, t.IsActive)).ToList();

        _tenantRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(tenants);

        _mapperMock.Setup(m => m.Map<IEnumerable<TenantListDto>>(tenants))
            .Returns(dtos);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        _tenantRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenTenantNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dto = new UpdateTenantDto("New Name", "new-subdomain");

        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId))
            .ReturnsAsync((Tenant?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateAsync(tenantId, dto));
        Assert.Equal("Tenant not found.", ex.Message);
        _tenantRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Tenant>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenSubdomainChangedAndAlreadyTaken_ThrowsInvalidOperationException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var existingTenant = new Tenant(Guid.NewGuid(), "Old Name", "old-subdomain");
        var dto = new UpdateTenantDto("New Name", "taken-subdomain");

        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId))
            .ReturnsAsync(existingTenant);

        _tenantRepoMock.Setup(r => r.ExistsBySubdomainAsync("taken-subdomain"))
            .ReturnsAsync(true);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateAsync(tenantId, dto));
        Assert.Equal("Subdomain is already in use.", ex.Message);
        _tenantRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Tenant>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenSubdomainUnchanged_DoesNotCheckSubdomainUniquenessAndUpdates()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var existingTenant = new Tenant(Guid.NewGuid(), "Old Name", "same-subdomain");
        var dto = new UpdateTenantDto("Updated Name", "same-subdomain");

        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId))
            .ReturnsAsync(existingTenant);

        var expectedDto = new TenantResponseDto(tenantId, existingTenant.PlanId, "Updated Name", "same-subdomain", true, existingTenant.CreatedAt, DateTime.UtcNow);

        _mapperMock.Setup(m => m.Map<TenantResponseDto>(existingTenant))
            .Returns(expectedDto);

        // Act
        var result = await _sut.UpdateAsync(tenantId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", existingTenant.TenantName);
        _tenantRepoMock.Verify(r => r.ExistsBySubdomainAsync(It.IsAny<string>()), Times.Never);
        _tenantRepoMock.Verify(r => r.UpdateAsync(existingTenant), Times.Once);
    }

    [Fact]
    public async Task ChangePlanAsync_WhenTenantNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dto = new ChangeTenantPlanDto(Guid.NewGuid());

        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId))
            .ReturnsAsync((Tenant?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.ChangePlanAsync(tenantId, dto));
        Assert.Equal("Tenant not found.", ex.Message);
    }

    [Fact]
    public async Task ChangePlanAsync_WhenPlanNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = new Tenant(Guid.NewGuid(), "Acme", "acme");
        var targetPlanId = Guid.NewGuid();
        var dto = new ChangeTenantPlanDto(targetPlanId);

        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId))
            .ReturnsAsync(tenant);

        _planRepoMock.Setup(r => r.GetByIdAsync(targetPlanId))
            .ReturnsAsync((Plan?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.ChangePlanAsync(tenantId, dto));
        Assert.Equal("Plan not found.", ex.Message);
        _tenantRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Tenant>()), Times.Never);
    }

    [Fact]
    public async Task ChangePlanAsync_WithValidPlan_UpdatesTenantPlanAndPersists()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var initialPlanId = Guid.NewGuid();
        var tenant = new Tenant(initialPlanId, "Acme", "acme");
        var targetPlanId = Guid.NewGuid();
        var targetPlan = new Plan("Premium", 500);
        var dto = new ChangeTenantPlanDto(targetPlanId);

        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId))
            .ReturnsAsync(tenant);

        _planRepoMock.Setup(r => r.GetByIdAsync(targetPlanId))
            .ReturnsAsync(targetPlan);

        // Act
        await _sut.ChangePlanAsync(tenantId, dto);

        // Assert
        Assert.Equal(targetPlanId, tenant.PlanId);
        _tenantRepoMock.Verify(r => r.UpdateAsync(tenant), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ChangeStatusAsync_WhenTenantFound_TogglesStatusAndPersists(bool newStatus)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = new Tenant(Guid.NewGuid(), "Acme", "acme");
        if (!newStatus)
        {
            // Initial is true, changing to false
            Assert.True(tenant.IsActive);
        }
        else
        {
            // Initial is set to false first, so we test activating
            tenant.Deactivate();
            Assert.False(tenant.IsActive);
        }

        var dto = new ChangeTenantStatusDto(newStatus);

        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId))
            .ReturnsAsync(tenant);

        // Act
        await _sut.ChangeStatusAsync(tenantId, dto);

        // Assert
        Assert.Equal(newStatus, tenant.IsActive);
        _tenantRepoMock.Verify(r => r.UpdateAsync(tenant), Times.Once);
    }

    [Fact]
    public async Task ChangeStatusAsync_WhenTenantNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId))
            .ReturnsAsync((Tenant?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.ChangeStatusAsync(tenantId, new ChangeTenantStatusDto(true)));
    }

    [Fact]
    public async Task DeleteAsync_WhenTenantNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId))
            .ReturnsAsync((Tenant?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(tenantId));
        _tenantRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenTenantFound_DeletesFromRepository()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = new Tenant(Guid.NewGuid(), "Acme", "acme");

        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId))
            .ReturnsAsync(tenant);

        // Act
        await _sut.DeleteAsync(tenantId);

        // Assert
        _tenantRepoMock.Verify(r => r.DeleteAsync(tenantId), Times.Once);
    }
}
