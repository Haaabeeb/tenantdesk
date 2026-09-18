using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using TenantDesk.Application.Dto.Sla;
using TenantDesk.Application.Interfaces;
using TenantDesk.Application.Interfaces.Identity;
using TenantDesk.Application.Services;
using TenantDesk.Domain.Entities;
using TenantDesk.Domain.Enums;
using TenantDesk.UnitTests.Helpers;
using Xunit;

namespace TenantDesk.UnitTests.Application;

public class SlaServiceTests
{
    private readonly Mock<ISlaRepository> _slaRepoMock;
    private readonly Mock<ICurrentTenantService> _currentTenantServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<SlaService>> _loggerMock;
    private readonly SlaService _sut;
    private readonly Guid _currentTenantId;

    public SlaServiceTests()
    {
        _slaRepoMock = new Mock<ISlaRepository>();
        _currentTenantServiceMock = new Mock<ICurrentTenantService>();
        _mapperMock = new Mock<IMapper>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<SlaService>>();

        _currentTenantId = Guid.NewGuid();
        _currentTenantServiceMock.Setup(c => c.GetTenantId()).Returns(_currentTenantId);

        _sut = new SlaService(
            _slaRepoMock.Object,
            _currentTenantServiceMock.Object,
            _mapperMock.Object,
            _memoryCache,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenResponseTimeGreaterThanResolutionTime_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateSlaDto(PriorityLevel.High, 10, 5); // 10 > 5

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(dto));
        Assert.Equal("Response time cannot be greater than resolution time.", ex.Message);
        _slaRepoMock.Verify(r => r.AddAsync(It.IsAny<Sla>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenSlaForPriorityAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateSlaDto(PriorityLevel.High, 2, 8);
        var existingSla = new Sla(_currentTenantId, PriorityLevel.High, 2, 8);

        _slaRepoMock.Setup(r => r.GetByTenantAndPriorityAsync(_currentTenantId, dto.Priority))
            .ReturnsAsync(existingSla);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(dto));
        Assert.Contains("already exists", ex.Message);
        _slaRepoMock.Verify(r => r.AddAsync(It.IsAny<Sla>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_CreatesSlaAndReturnsDto()
    {
        // Arrange
        var dto = new CreateSlaDto(PriorityLevel.Critical, 1, 4);

        _slaRepoMock.Setup(r => r.GetByTenantAndPriorityAsync(_currentTenantId, dto.Priority))
            .ReturnsAsync((Sla?)null);

        var expectedDto = new SlaResponseDto(Guid.NewGuid(), PriorityLevel.Critical, 1, 4, DateTime.UtcNow, null);
        _mapperMock.Setup(m => m.Map<SlaResponseDto>(It.IsAny<Sla>()))
            .Returns(expectedDto);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Priority, result.Priority);
        _slaRepoMock.Verify(r => r.AddAsync(It.Is<Sla>(s => s.TenantId == _currentTenantId && s.Priority == dto.Priority)), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenSlaNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var slaId = Guid.NewGuid();
        _slaRepoMock.Setup(r => r.GetByIdAsync(slaId))
            .ReturnsAsync((Sla?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(slaId));
        Assert.Equal("SLA not found.", ex.Message);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBelongingToDifferentTenant_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var otherTenantId = Guid.NewGuid();
        var slaId = Guid.NewGuid();
        var sla = new Sla(otherTenantId, PriorityLevel.Low, 24, 48);
        TestEntityHelper.SetProperty(sla, "SlaId", slaId);

        _slaRepoMock.Setup(r => r.GetByIdAsync(slaId))
            .ReturnsAsync(sla);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.GetByIdAsync(slaId));
        Assert.Equal("You cannot access resources belonging to another tenant.", ex.Message);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFoundAndOwned_ReturnsMappedDto()
    {
        // Arrange
        var slaId = Guid.NewGuid();
        var sla = new Sla(_currentTenantId, PriorityLevel.Medium, 8, 16);
        TestEntityHelper.SetProperty(sla, "SlaId", slaId);

        _slaRepoMock.Setup(r => r.GetByIdAsync(slaId))
            .ReturnsAsync(sla);

        var expectedDto = new SlaResponseDto(slaId, PriorityLevel.Medium, 8, 16, sla.CreatedAt, null);
        _mapperMock.Setup(m => m.Map<SlaResponseDto>(sla))
            .Returns(expectedDto);

        // Act
        var result = await _sut.GetByIdAsync(slaId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(slaId, result.SlaId);
        Assert.Equal(PriorityLevel.Medium, result.Priority);
    }

    [Fact]
    public async Task GetByPriorityAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _slaRepoMock.Setup(r => r.GetByTenantAndPriorityAsync(_currentTenantId, PriorityLevel.High))
            .ReturnsAsync((Sla?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByPriorityAsync(PriorityLevel.High));
    }

    [Fact]
    public async Task GetByPriorityAsync_WhenFound_ReturnsMappedDto()
    {
        // Arrange
        var sla = new Sla(_currentTenantId, PriorityLevel.High, 2, 6);
        _slaRepoMock.Setup(r => r.GetByTenantAndPriorityAsync(_currentTenantId, PriorityLevel.High))
            .ReturnsAsync(sla);

        var expectedDto = new SlaResponseDto(Guid.NewGuid(), PriorityLevel.High, 2, 6, sla.CreatedAt, null);
        _mapperMock.Setup(m => m.Map<SlaResponseDto>(sla))
            .Returns(expectedDto);

        // Act
        var result = await _sut.GetByPriorityAsync(PriorityLevel.High);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PriorityLevel.High, result.Priority);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedSlaList()
    {
        // Arrange
        var slas = new List<Sla>
        {
            new Sla(_currentTenantId, PriorityLevel.Low, 24, 72),
            new Sla(_currentTenantId, PriorityLevel.High, 2, 8)
        };

        var dtos = slas.Select(s => new SlaResponseDto(Guid.NewGuid(), s.Priority, s.ResponseTime, s.ResolutionTime, s.CreatedAt, null)).ToList();

        _slaRepoMock.Setup(r => r.GetAllByTenantAsync(_currentTenantId))
            .ReturnsAsync(slas);

        _mapperMock.Setup(m => m.Map<IEnumerable<SlaResponseDto>>(slas))
            .Returns(dtos);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        _slaRepoMock.Verify(r => r.GetAllByTenantAsync(_currentTenantId), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenResponseTimeGreaterThanResolutionTime_ThrowsArgumentException()
    {
        // Arrange
        var slaId = Guid.NewGuid();
        var dto = new UpdateSlaTimesDto(20, 10); // 20 > 10

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateAsync(slaId, dto));
        Assert.Equal("Response time cannot be greater than resolution time.", ex.Message);
        _slaRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Sla>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenSlaFromDifferentTenant_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var slaId = Guid.NewGuid();
        var otherSla = new Sla(Guid.NewGuid(), PriorityLevel.Medium, 4, 8);
        var dto = new UpdateSlaTimesDto(2, 6);

        _slaRepoMock.Setup(r => r.GetByIdAsync(slaId))
            .ReturnsAsync(otherSla);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.UpdateAsync(slaId, dto));
        _slaRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Sla>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithValidTimes_UpdatesTimesAndPersists()
    {
        // Arrange
        var slaId = Guid.NewGuid();
        var sla = new Sla(_currentTenantId, PriorityLevel.Low, 12, 24);
        TestEntityHelper.SetProperty(sla, "SlaId", slaId);
        var dto = new UpdateSlaTimesDto(6, 12);

        _slaRepoMock.Setup(r => r.GetByIdAsync(slaId))
            .ReturnsAsync(sla);

        var expectedDto = new SlaResponseDto(slaId, PriorityLevel.Low, 6, 12, sla.CreatedAt, DateTime.UtcNow);
        _mapperMock.Setup(m => m.Map<SlaResponseDto>(sla))
            .Returns(expectedDto);

        // Act
        var result = await _sut.UpdateAsync(slaId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(6, sla.ResponseTime);
        Assert.Equal(12, sla.ResolutionTime);
        _slaRepoMock.Verify(r => r.UpdateAsync(sla), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenSlaFromDifferentTenant_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var slaId = Guid.NewGuid();
        var otherSla = new Sla(Guid.NewGuid(), PriorityLevel.Low, 12, 24);

        _slaRepoMock.Setup(r => r.GetByIdAsync(slaId))
            .ReturnsAsync(otherSla);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.DeleteAsync(slaId));
        _slaRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenFoundAndOwned_DeletesFromRepository()
    {
        // Arrange
        var slaId = Guid.NewGuid();
        var sla = new Sla(_currentTenantId, PriorityLevel.Low, 12, 24);

        _slaRepoMock.Setup(r => r.GetByIdAsync(slaId))
            .ReturnsAsync(sla);

        // Act
        await _sut.DeleteAsync(slaId);

        // Assert
        _slaRepoMock.Verify(r => r.DeleteAsync(slaId), Times.Once);
    }
}
